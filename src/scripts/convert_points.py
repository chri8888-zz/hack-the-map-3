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

input = open('output-crowd-sim-1.csv', 'r')
output = open('output-crowd-sim-1-converted.csv', 'w')

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