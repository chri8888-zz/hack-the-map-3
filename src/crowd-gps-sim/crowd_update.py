import http.client
import urllib
import json
import requests

server = 'https://cardiffportal.esri.com'
layer = '/server/rest/services/Hosted/FestiFeatureService/FeatureServer/0/'
port = 6443

def get_token(username, password, url):
  # Token URL is typically http://server[:port]/arcgis/admin/generateToken
    tokenURL = "/portal/sharing/rest/generateToken/"
    
    request_param = {
      'username': username, 
      'password': password, 
      'client': 'referer', 
      'referer': 'https://cardiffportal.esri.com:6443/arcgis/admin', 
      'f': 'pjson', 
      'expiration': '10'
    }

    form = {
      'f': request_param
    }

    json_str = json.dumps(form)

    # URL-encode the token parameters
    params = urllib.parse.urlencode(form)

    headers = {"Content-type": "application/x-www-form-urlencoded", "Accept": "text/plain"}

    request_url = server + tokenURL

    # Read response
    response = requests.post(request_url, headers=headers, data=request_param, verify=False)
    # response = requests.post(request_url, params=params, headers=headers, data=request_params, verify=False)
    if (response.status_code != 200):
        print("Error while fetching tokens from admin URL. Please check the URL and try again.")
        return
    else:
        data = response.text
        # Extract the token from it
        token = json.loads(data)        
        return token["token"]         

def send_features(features, request_url, token):
  params = urllib.parse.urlencode({'token': token, 'f': 'pjson'})
  headers = {"Content-type": "application/x-www-form-urlencoded", "Accept": "text/plain"}

  features_json = json.dumps(features)
  if len(features_json) == 0:
    exit()

  url = server + layer + request_url

  response = requests.post(url, params=params, headers=headers, data={'features': features_json}, verify=False)
  if (response.status_code != 200):
    print("Failed to update feature")
    return
  else:
    data = response.text
    print(data)

def create_feature(oid, track_id, x = 0.0, y = 0.0, date_time = None, velocity = 0.0, distance = 0.0):
  return {
    'attributes': {
      'oid': oid,
      'id': track_id,
      'x': x,
      'y': y,
      'date_time': date_time,
      'velocity': velocity,
      'distance': distance
    },
    'geometry': {
      'x': x,
      'y': y
    }
  }

def send_request(feature):
  login = open('./login.txt', 'r')
  lines = login.readlines()
  user = lines[0].strip()
  passwd = lines[1].strip()

  features = []
  for i in range(1, 10):
    feature = create_feature(i, i)
    features.append(feature)

  token = get_token(user, passwd, server)
  send_features(features, 'addFeatures', token)

if __name__ == "__main__":
  feature = create_feature(1, 1)
  send_request(feature)