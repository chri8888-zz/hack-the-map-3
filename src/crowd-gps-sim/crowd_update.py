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

def send_users(features, request_url):
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

def query_users():
  url = server + layer + 'query'

  features_json = http_post(url, {'where': 'oid > 0', 'outfields': '*'})
  if len(features_json) == 0:
    print("Failed to query features")
    return 

  query_result = json.loads(features_json)
  if 'features' in query_result.keys() == False:
    print("Failed to query features")
    return None
  
  users = query_result['features']

  user_records = {}
  for user in users:
    if 'attributes' in user.keys() == False:
      continue

    attributes = user['attributes']    
    user_id = attributes['id']
    oid = attributes['oid']
    x = attributes['x']
    y = attributes['y']
    date_time = attributes['date_time']
    velocity = attributes['velocity']
    distance = attributes['distance']

    user_record = create_user(user_id, x, y, date_time, velocity, distance, oid)
    user_records[user_id] = user_record

  return user_records

def create_user(track_id, x = 0.0, y = 0.0, date_time = None, velocity = 0.0, distance = 0.0, oid = None):
  user = {
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

  if oid != None:
    user['attributes']['oid'] = oid

def read_csv(filename):
  f = open(filename, 'r')

  users = []
  for line in f.readlines():
    attributes = line.split(',')

    id = attributes[0]
    user = create_user(id, attributes[1], attributes[2], attributes[3], attributes[4], attributes[5])
    users.append(user)

  return users

def create_users():
  users = []
  for i in range(1, USER_COUNT):
    feature = create_user(i)
    features.append(feature)

  send_features(features, 'addFeatures')  

if __name__ == "__main__":
  global token
  
  login = open('./login.txt', 'r')
  lines = login.readlines()
  user = lines[0].strip()
  passwd = lines[1].strip()

  token = get_token(user, passwd, server)
  current_users = query_users()

  current_user_count = len(current_users)
  if current_user_count == 0:
    print('creating users please restart')
    features = []
    for i in range(0, USER_COUNT - 1):
      feature = create_feature(i)
      features.append(feature)

    send_features(features, 'addFeatures')
    exit()
  elif current_user_count != USER_COUNT - 1:
    print("Invalid number of users. Deleting. Please restart")
    exit() 

  user_positions = read_csv(CSV_FILE)
  # print(len(user_positions))

