#!/usr/bin/python
import codecs
import yaml
import random
import math


def interpolate(start, end, t):
    (a_x, a_y) = start
    (b_x, b_y) = end
    x = (a_x + t * (b_x - a_x))
    y = (a_y + t * (b_y - a_y))
    return (x, y)


def point_distance(start, end):
    (a_x, a_y) = start
    (b_x, b_y) = end
    diff_x = (b_x - a_x) * (b_x - a_x)
    diff_y = (b_y - a_y) * (b_y - a_y)

    return math.sqrt(diff_x + diff_y)


def load(path):
    # Open yaml
    stream = codecs.open(path, "r", encoding="utf-8")
    yaml_load = yaml.load(stream.read())

    # Start points
    points_start = []
    for point in yaml_load["starting-locations"]:
        points_start.append((point["x"], point["y"]))

    # Points of interest
    points_interest = []
    for point in yaml_load["points-of-interest"]:
        points_interest.append((point["x"], point["y"], point["name"]))

    # Crowd Size + max visits
    crowd_size = int(yaml_load["crowd-size"])
    max_visits = int(yaml_load["max-visits"])
    pulse_resolution = int(yaml_load["pulse-resolution"])
    m_to_unit = float(yaml_load["meters-to-unit"])
    avg_vel = float(yaml_load["vel"])
    vel_error = float(yaml_load["vel-error"])

    return (points_start, points_interest, crowd_size, max_visits, pulse_resolution, m_to_unit, avg_vel, vel_error)


(points_start, points_interest, crowd_size, max_visits,
 pulse_resolution, m_to_unit, avg_vel, vel_error) = load("settings.yaml")

print(points_start)
print(points_interest)
print(crowd_size)

# Process
for i in range(crowd_size):
    print("--")

    visits = random.randrange(1, max_visits)
    to_visit = []

    # Build the visit stack
    for j in range(visits):
        to_visit.append(random.randrange(0, len(points_interest)))

    # Choose starting location
    current_point = points_start[random.randrange(len(points_start))]

    # Walk route
    route_stringer = []
    route_stringer.append("x,y,time,velocity,distance")
    total_traverse_time = 0
    total_traverse_distance = 0
    for visit_index in to_visit:
        target_point = points_interest[visit_index]

        target_xy = (target_point[0], target_point[1])
        current_xy = (current_point[0], current_point[1])
        distance = point_distance(current_xy, target_xy) * m_to_unit
        calculated_vel = avg_vel + random.uniform(-vel_error, vel_error)
        time_to_travese = distance / calculated_vel

        total_traverse_time += time_to_travese
        total_traverse_distance += distance

        str_x = str(current_point[0])
        str_y = str(current_point[1])
        str_time = str(total_traverse_time)
        str_vel = str(calculated_vel)
        str_distance = str(total_traverse_distance)

        string_build = str_x + "," + str_y + "," + \
            str_time + "," + str_vel + "," + str_distance
        route_stringer.append(string_build)
        current_point = target_point

    print(route_stringer)

    # Simulate pulse
    # max_interpolations = random.randrange(pulse_resolution)
##
    # print(interpolate((0, 0), (8, 4), 0.3))
