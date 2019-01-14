//package com.festiflo.festiflow_minion;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.URL;
import java.net.MalformedURLException;
import java.net.HttpURLConnection;
import java.util.*;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.cert.X509Certificate;

public class RestRequest {
  
  private static String cardiffServer = "https://cardiffportal.esri.com";
  
  private static String postHttpRequest(String urlString, Map<String, Object> paramMap) {
     try {
      
      TrustManager[] trustAllCerts = new TrustManager[] { new X509TrustManager() {
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
      
      StringBuilder postData = new StringBuilder();
      for (Map.Entry<String, Object> param : params.entrySet()) {
        if (postData.length() != 0) 
          postData.append('&');
        
        postData.append(URLEncoder.encode(param.getKey(), "UTF-8"));
        postData.append('=');
        postData.append(URLEncoder.encode(String.valueOf(param.getValue()), "UTF-8"));
      }
      byte[] postDataBytes = postData.toString().getBytes("UTF-8");
      
      HttpsURLConnection.setDefaultHostnameVerifier(allHostsValid);
      URL url = new URL(urlString);
      HttpURLConnection conn = (HttpURLConnection) url.openConnection();
      conn.setRequestMethod("POST");
      conn.setRequestProperty("Content-type", "application/x-www-form-urlencoded");
      conn.setRequestProperty("Accept", "text/plain");      
      conn.setRequestProperty("Content-Length", String.valueOf(postDataBytes.length));
      conn.setDoOutput(true);
      conn.getOutputStream().write(postDataBytes);
      
      if (conn.getResponseCode() != 200) {
        System.out.println("Failed to connect");
        System.out.println(conn.getResponseCode());
        return "";
      }

      BufferedReader br = new BufferedReader(new InputStreamReader((conn.getInputStream())));
      
      StringBuffer sb = new StringBuffer();
      String output;
      while ((output = br.readLine()) != null) {
        sb.append(output);
      }
      
      return sb.toString();
    }
    catch (Exception e) {
      System.out.println(e);
    }
    
    return "";
  }
  
  
  private static String getToken() {
    String tokenUrl = "/portal/sharing/rest/generateToken/";
    
    Map<String, Object> params = new LinkedHashMap<>();
    params.put("username", "shawker");
    params.put("password", "Festiflo1");
    params.put("client", "referer");
    params.put("referer", "https://cardiffportal.esri.com:6443/arcgis/admin");
    params.put("f", "pjson");
    params.put("expiration", "10000");
    
    String result = postHttpRequest(cardiffServer + tokenUrl, params);
    if (result == null || result.equals("")) {
      System.out.println("Failed to get token");
      return "";
    }
    
    return result;
  }
  
  public static void main(String[] args) {

  /*
    Map<String, Object> params = new LinkedHashMap<>();
    //params.put("");

    HttpRequest("https://cardiffportal.esri.com/server/rest/services/Hosted/EventServiceWebMercator/FeatureServer/0/query", params);
    */
    
    String token = getToken();
    System.out.println(token);
  }
}