#!/usr/bin/python
import codecs
import csv
import argparse


def load_points_from_csv(name):
    path = "./" + name + ".csv"
    points = []
    with open(path) as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            x = float(row["X"])
            y = float(row["Y"])
            radius = float(row["Radius"])
            new_interset_point = (x, y, radius)
            points.append(new_interset_point)

    return points


def empty(args):
    print("use -h or --help for help")


def convert(path):
    points = []
    with open(path) as csvfile:
        reader = csv.DictReader(csvfile)
        for row in reader:
            x = float(row["X"])
            y = float(row["Y"])
            radius = float(row["Radius"])
            new_interset_point = (x, y, radius)
            points.append(new_interset_point)

    print(points)

    stringer = "<gpx><trk><trkseg>"
    for point in points:
        stringer += '<trkpt lon="' + \
            str(point[0]) + 'lat=' + str(point[1]) + '></trkpt >'
    stringer += "</trkseg></trk></gpx>"

    file = open("out.gpx", "w")
    file.write(stringer)


convert("in.csv")
