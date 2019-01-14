package com.festiflo.festiflow_minion;

import android.util.Log;

import java.io.*;
import java.net.*;
import java.util.*;

import javax.net.ssl.*;

import java.security.cert.X509Certificate;

public class RestRequest {

    public RestRequest() {

        // Our certificates are invalid, so we need to bypass certificate validation
        try {
            TrustManager[] trustAllCerts = new TrustManager[]{new X509TrustManager() {
                public java.security.cert.X509Certificate[] getAcceptedIssuers() {
                    return null;
                }

                public void checkClientTrusted(X509Certificate[] certs, String authType) {

                }

                public void checkServerTrusted(X509Certificate[] certs, String authType) {

                }
            }
            };

            // Install the all-trusting trust manager
            SSLContext sc = SSLContext.getInstance("SSL");
            sc.init(null, trustAllCerts, new java.security.SecureRandom());
            HttpsURLConnection.setDefaultSSLSocketFactory(sc.getSocketFactory());

            // Create all-trusting host name verifier
            HostnameVerifier allHostsValid = new HostnameVerifier() {
                public boolean verify(String hostname, SSLSession session) {
                    return true;
                }
            };

            HttpsURLConnection.setDefaultHostnameVerifier(allHostsValid);
        } catch (Exception e) {
            System.out.println(e);
        }
    }

    private String postHttpRequest(String urlString, Map<String, Object> paramMap) {
        try {
            // Build and encode the post content
            StringBuilder postData = new StringBuilder();
            for (Map.Entry<String, Object> param : paramMap.entrySet()) {
                if (postData.length() != 0)
                    postData.append('&');

                postData.append(URLEncoder.encode(param.getKey(), "UTF-8"));
                postData.append('=');
                postData.append(URLEncoder.encode(String.valueOf(param.getValue()), "UTF-8"));
            }
            byte[] postDataBytes = postData.toString().getBytes("UTF-8");

            // Setup the post headers and content
            URL url = new URL(urlString);
            HttpURLConnection urlConnection = (HttpURLConnection) url.openConnection();

            urlConnection.setRequestMethod("POST");
            urlConnection.setRequestProperty("Content-type", "application/x-www-form-urlencoded");
            urlConnection.setRequestProperty("Accept", "text/plain");
            //urlConnection.setRequestProperty("Content-Length", String.valueOf(postDataBytes.length));
            urlConnection.setUseCaches(false);
            urlConnection.setAllowUserInteraction(false);
            urlConnection.setDoOutput(true);
            urlConnection.setDoInput(true);
            OutputStream out = new BufferedOutputStream(urlConnection.getOutputStream());
            out.write(postDataBytes);

            int responceCode = urlConnection.getResponseCode();
            if (responceCode == HttpURLConnection.HTTP_OK) {
                BufferedReader br = new BufferedReader(new InputStreamReader((urlConnection.getInputStream())));
                StringBuffer sb = new StringBuffer();
                String output;
                while ((output = br.readLine()) != null) {
                    sb.append(output);
                }

                urlConnection.disconnect();

                return sb.toString();
            }
        } catch (Exception e) {
            Log.e("Failed to post HTTP request", e.getMessage());
        }

        return "";
    }

    private String getToken() {

        //return "2oxPcK8tehXJLe9B8p1FlF1cJyl9Slj7mavZkKJxflul0QfISINSgrQWAWZzn8l-KkIw6D81neGDmq4tW10j2dBZqSzKugUq-fTnk81eHmfCRUA7Vfl7SZXfQVfrHkLulI1RJcmzkk54nNNavMptF5-9Z0ktXYSYTrcCbyX9qp4.";

        String cardiffServer = "https://cardiffportal.esri.com";
        String tokenUrl = "/portal/sharing/rest/generateToken/";

        Map<String, Object> params = new LinkedHashMap<>();
        params.put("username", "shawker");
        params.put("password", "Festiflo1");
        params.put("client", "referer");
        params.put("referer", "https://cardiffportal.esri.com:6443/arcgis/admin");
        params.put("f", "json");
        params.put("expiration", "10000");

        String result = postHttpRequest(cardiffServer + tokenUrl, params);
        if (result == null || result.equals("")) {
            Log.e("Failed to get Token", "");
            return "";
        }

        // ToDo Parse JSON

        return result;
    }

    public void updateUserPostion(int userID, double x, double y) {
        String token = getToken();
        String cardiffServer = "https://cardiffportal.esri.com";
        String usersLayer = "/server/rest/services/Hosted/FestiFlowUserService/FeatureServer/0/";


        Map<String, Object> queryParams = new LinkedHashMap<>();
        queryParams.put("where", "id = " + Integer.toString(userID));
        queryParams.put("outfields", "*");
        queryParams.put("token", token);
        queryParams.put("f", "pjson");
        String userData = postHttpRequest(cardiffServer + usersLayer + "query", queryParams);

        // ToDo: parse json

        String usersJSONArray = "[]";

        Map<String, Object> updateParams = new LinkedHashMap<>();
        updateParams.put("token", token);
        updateParams.put("f", "pjson");
        updateParams.put("updates", usersJSONArray);
        postHttpRequest(cardiffServer + usersLayer + "applyEdits", updateParams);
    }

    public String queryEvents() {
        String token = getToken();
        String cardiffServer = "https://cardiffportal.esri.com";
        String eventLayer = "/server/rest/services/Hosted/EventServiceWebMercator/FeatureServer/0/";

        Map<String, Object> queryParams = new LinkedHashMap<>();
        queryParams.put("token", token);
        queryParams.put("f", "pjson");
        queryParams.put("where", "oid > 0");
        queryParams.put("outfields", "*");

        String result = postHttpRequest(cardiffServer + eventLayer + "query", queryParams);
        return result;
    }

    public static void main(String[] args) {
        RestRequest request = new RestRequest();
        String token = request.getToken();
        //System.out.println(request.updateUserPostion(3, 0.0, 0.0));
        System.out.println(request.queryEvents());
    }
}