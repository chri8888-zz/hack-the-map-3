#!/usr/bin/python
import codecs
import yaml
import random
import math
import time
from datetime import datetime, date, time, timedelta
from random import shuffle
import csv


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


def location_drift(error):
    drift_x = random.uniform(-error, error)
    drift_y = random.uniform(-error, error)
    return (drift_x, drift_y)


def tuple_addition(left, right):
    return (left[0] + right[0], left[1] + right[1])


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


def export(path, events):
    route_stringer = "id,x,y,time,velocity,distance\n"
    for event in events:
        # Event to string
        (id, str_x, str_y, str_time, str_vel, str_distance) = event
        str_id = str(id)
        str_x = str(str_x)
        str_y = str(str_y)
        str_time = str(str_time)
        str_vel = str(str_vel)
        str_distance = str(str_distance)

        string_build = str_id + "," + str_x + "," + str_y + "," + \
            str_time + "," + str_vel + "," + str_distance
        route_stringer += string_build + "\n"

    file = open(path, "w")
    file.write(route_stringer)


def load(path):
    # Open yaml
    stream = codecs.open(path, "r", encoding="utf-8")
    yaml_load = yaml.load(stream.read())

    # Pointspoints_start
    points_start = []
    for file_name in yaml_load["starting-locations"]:
        points_start += load_points_from_csv(file_name)

    points_interest = []
    for file_name in yaml_load["points-of-interest"]:
        points_interest += load_points_from_csv(file_name)

    # Crowd Size + max visits
    crowd_size = int(yaml_load["crowd-size"])
    max_visits = int(yaml_load["max-visits"])
    pulse_resolution = int(yaml_load["pulse-resolution"])
    pulse_resolution_error = int(yaml_load["pulse-resolution-error"])
    m_to_unit = float(yaml_load["meters-to-unit"])
    location_error = float(yaml_load["location-error"])
    start_time_range = float(yaml_load["start-time-range"])
    avg_vel = float(yaml_load["vel"])
    vel_error = float(yaml_load["vel-error"])
    wait_min = int(yaml_load["wait-min"])
    wait_max = int(yaml_load["wait-max"])
    start_date_time = datetime(
        int(yaml_load["start-date-year"]),
        int(yaml_load["start-date-month"]),
        int(yaml_load["start-date-day"]),
        int(yaml_load["start-date-hour"]),
        int(yaml_load["start-date-minute"]),
        int(yaml_load["start-date-second"]))

    return (points_start, points_interest, crowd_size, max_visits, pulse_resolution, pulse_resolution_error, m_to_unit, location_error, start_time_range, avg_vel, vel_error, start_date_time, wait_min, wait_max)


(points_start, points_interest, crowd_size, max_visits,
 pulse_resolution, pulse_resolution_error, m_to_unit, location_error, start_time_range, avg_vel, vel_error, start_date_time, wait_min, wait_max) = load("settings.yaml")

# print(points_start)
# print(points_interest)
# print(crowd_size)

print("Start simulation")

device_pulse_events = []

# Process
for id in range(crowd_size):
    to_visit = []

    # Shuffle the visit stack
    to_visit = [i for i in range(len(points_interest))]
    shuffle(to_visit)
    to_visit = to_visit[:(random.randrange(1, max_visits))]

    # Choose starting location
    start_point = points_start[random.randrange(len(points_start))]
    start_point_xy = tuple_addition(
        start_point, location_drift(location_error))

    # Interpolate pulse events
    pulse_events = []
    pulse_events.append(start_point_xy)
    previous_point = start_point_xy
    for visit_index in to_visit:
        target_point = points_interest[visit_index]

        # GPS sim
        target_xy = tuple_addition(
            target_point, location_drift(location_error))

        # Generate the number of GPS syncs
        pulse_count = pulse_resolution + \
            random.randrange(-pulse_resolution_error,
                             pulse_resolution_error)

        # Interpolate across the path for pulse events
        for pulse_index in range(pulse_count):
            point = interpolate(previous_point, target_xy,
                                random.uniform(0, 1))
            pulse_events.append(tuple_addition(
                point, location_drift(location_error)))

        # Simulate wait
        wait_loops = random.randrange(wait_min, wait_max)
        for i in range(wait_loops):
            # Set point in radius
            radius = target_point[2]
            wait_offset = location_drift(radius)
            wait_point = tuple_addition(
                target_xy, wait_offset)
            pulse_events.append(tuple_addition(
                wait_point, location_drift(location_error)))

        previous_point = target_xy
        pulse_events.append(target_xy)

    # Walk route
    total_traverse_time = start_date_time + timedelta(
        seconds=random.uniform(0, start_time_range))
    total_traverse_distance = 0

    for pulse_event_index in range(len(pulse_events)):
        current_point = pulse_events[pulse_event_index]
        distance = 0
        time_to_traverse = 0
        calculated_vel = 0

        # Calculate vel from distance
        if pulse_event_index > 0:
            previous_point = pulse_events[pulse_event_index - 1]
            distance = point_distance(
                current_point, previous_point) * m_to_unit
            calculated_vel = avg_vel + random.uniform(-vel_error, vel_error)
            time_to_traverse = distance / calculated_vel

        # Trip stats
        total_traverse_time += timedelta(seconds=time_to_traverse)
        total_traverse_distance += distance

        # Add to the list of events
        device_pulse_events.append(
            (id, current_point[0], current_point[1], total_traverse_time, calculated_vel, total_traverse_distance))

print("Start export")

# Export sorted events to CSV
export("output-crowd-sim.csv", sorted(device_pulse_events,
                                      key=lambda time_stamp: time_stamp[3]))
print("Complete")
