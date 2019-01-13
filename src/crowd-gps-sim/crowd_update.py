import http.client
import urllib
import json
import requests

server = 'https://cardiffportal.esri.com'
layer = '/server/rest/services/Hosted/FestiFeatureService/FeatureServer/0/'
port = 6443

USER_COUNT = 100
CSV_FILE = './output_crowd_sim-100.csv'

def http_post(url, data, post_params = None):
  global token

  if post_params == None:
    post_params = {'token': token, 'f': 'pjson'}

  params = urllib.parse.urlencode(post_params)
  headers = {"Content-type": "application/x-www-form-urlencoded", "Accept": "text/plain"}

  response = requests.post(url, params = params, headers = headers, data = data, verify = False)
  if (response.status_code != 200):
    print("Request failed " + url)
    return None

  return response.text

def get_token(username, password, url):
  # Token URL is typically http://server[:port]/arcgis/admin/generateToken
    tokenURL = "/portal/sharing/rest/generateToken/"
    
    request_param = {
      'username': username, 
      'password': password, 
      'client': 'referer', 
      'referer': 'https://cardiffportal.esri.com:6443/arcgis/admin', 
      'f': 'pjson', 
      'expiration': '1000'
    }

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

def send_features(features, request_url):
  params = urllib.parse.urlencode({'token': token, 'f': 'pjson'})
  headers = {"Content-type": "application/x-www-form-urlencoded", "Accept": "text/plain"}

  features_json = json.dumps(features)
  if len(features_json) == 0:
    exit()

  url = server + layer + request_url

  response = requests.post(url, params=params, headers=headers, data={'features': features_json}, verify=False)
  if (response.status_code != 200):
    print("Failed to update feature")
  #   return
  # else:
  #   data = response.text

def query_features():
  url = server + layer + 'query'

  features_json = http_post(url, {'where': 'oid > 0'})
  if len(features_json) == 0:
    print("Failed to query features")
    return 

  query_result = json.loads(features_json)
  print(features_json)
  features = query_result['features']
  if features == None:
    return

  print(len(features))

def create_feature(track_id, x = 0.0, y = 0.0, date_time = None, velocity = 0.0, distance = 0.0):
  return {
    'attributes': {
      # 'oid': oid,
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

def read_csv(filename):
  f = open(filename, 'r')

  features = []
  for line in f.readlines():
    attributes = line.split(',')

    id = attributes[0]
    feature = create_feature(id, attributes[1], attributes[2], attributes[3], attributes[4], attributes[5])
    features.append(feature)

  return features

def create_users():
  users = []
  for i in range(1, USER_COUNT):
    feature = create_feature(i)
    features.append(feature)

  send_features(features, 'addFeatures')  

if __name__ == "__main__":
  global token
  feature = create_feature(1, 1)
  
  login = open('./login.txt', 'r')
  lines = login.readlines()
  user = lines[0].strip()
  passwd = lines[1].strip()

  token = get_token(user, passwd, server)
  current_features = query_features()

  # features = []
  # for i in range(0, USER_COUNT - 1):
  #   feature = create_feature(i)
  #   features.append(feature)

  # //send_features(features, 'addFeatures')

  # user_positions = read_csv()
  # print(len(user_positions))

