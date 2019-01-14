import arcpy

inSR = arcpy.SpatialReference(3857)
outSR = arcpy.SpatialReference(4326)

def convert_point(x, y):
  pt = arcpy.Point()
  pt.X = x
  pt.Y = y
  pt_geo = arcpy.PointGeometry(pt, inSR)
  pt_geo1 = pt_geo.projectAs(outSR)
  pt1 = pt_geo1.lastPoint
  return [pt1.X, pt1.Y]

def convert_simulation_data():
    input = open('output-crowd-sim-GOOD.csv', 'r')
    output = open('output-crowd-sim-GOOD-converted.csv', 'w')

    lines = input.readlines()
    output.write(lines[0])

    for line in lines[1:]:
        attributes = line.split(',')
        x = attributes[1]
        y = attributes[2]
        result = convert_point(x, y)
        attributes[1] = result[0]
        attributes[2] = result[1]

        str_buff = attributes[0]
        for i in range(1, len(attributes)):
            str_buff += ',' + str(attributes[i])
        
        output.write(str_buff)

    input.close()
    output.close()

def get_unique():
    input = open('output-crowd-sim-GOOD-converted.csv', 'r')
    output = open('output_crowd-sim-starting.csv', 'w')

    users = {}

    for line in input.readlines()[1:]:
        attributes = line.split(',')
        id = attributes[0]

        if id in users:
            continue
        
        users[id] = line

    for user in users.keys():
        output.write(users[user])

    input.close()
    output.close()

if __name__ == "__main__":
    get_unique()