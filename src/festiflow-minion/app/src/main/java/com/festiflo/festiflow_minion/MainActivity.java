package com.festiflo.festiflow_minion;

import android.Manifest;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.annotation.RequiresApi;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.View;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Toast;

import com.esri.arcgisruntime.data.ServiceFeatureTable;
import com.esri.arcgisruntime.geometry.Point;
import com.esri.arcgisruntime.geometry.PointCollection;
import com.esri.arcgisruntime.geometry.SpatialReference;
import com.esri.arcgisruntime.geometry.SpatialReferences;
import com.esri.arcgisruntime.layers.FeatureLayer;
import com.esri.arcgisruntime.mapping.ArcGISMap;
import com.esri.arcgisruntime.mapping.Basemap;
import com.esri.arcgisruntime.mapping.Viewpoint;
import com.esri.arcgisruntime.mapping.view.LocationDisplay;
import com.esri.arcgisruntime.mapping.view.MapView;

import java.util.List;

public class MainActivity extends AppCompatActivity {

    String[] reqPermissions = new String[]{Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission
            .ACCESS_COARSE_LOCATION};
    private MapView mMapView;
    private LocationDisplay mLocationDisplay;
    private int requestCode = 2;

    private final int mUpdateSpeed = 1000 * 5;
    private long mUpdateTimer = 0;
    private Stopwatch mGPSDelta = new Stopwatch();
    private PointCollection mPointsBuffer = new PointCollection(SpatialReferences.getWgs84());

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                mLocationDisplay.setAutoPanMode(LocationDisplay.AutoPanMode.RECENTER);
            }
        });

        mMapView = findViewById(R.id.mapView);

        // Set initial location
        ArcGISMap map = new ArcGISMap(SpatialReferences.getWebMercator());
        map.setBasemap(Basemap.createImageryWithLabels());
        //Point glasto_location = new Point(-290580.7608083967, 6649146.157472552, SpatialReferences.getWebMercator());
        //map.setInitialViewpoint(new Viewpoint(glasto_location, 20000));
        //map.setInitialViewpoint(new Viewpoint(new Point(-13176752, 4090404, SpatialReferences.getWebMercator()), 500000));

        // add the layers to the map
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_campsites))));
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_carparks))));
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_entrances))));
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_toilets))));
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_stages))));
        map.getOperationalLayers().add(new FeatureLayer(new ServiceFeatureTable(
                getResources().getString(R.string.url_events))));

        mMapView.setMap(map);

        // get the MapView's LocationDisplay
        mLocationDisplay = mMapView.getLocationDisplay();
        mLocationDisplay.setAutoPanMode(LocationDisplay.AutoPanMode.COMPASS_NAVIGATION);
        mLocationDisplay.startAsync();

        // Listen to changes in the status of the location data source.
        mLocationDisplay.addDataSourceStatusChangedListener(new LocationDisplay.DataSourceStatusChangedListener() {
            @Override
            public void onStatusChanged(LocationDisplay.DataSourceStatusChangedEvent dataSourceStatusChangedEvent) {

                // If LocationDisplay started OK, then continue.
                if (dataSourceStatusChangedEvent.isStarted())
                    return;

                // No error is reported, then continue.
                if (dataSourceStatusChangedEvent.getError() == null)
                    return;

                // If an error is found, handle the failure to start.
                // Check permissions to see if failure may be due to lack of permissions.
                boolean permissionCheck1 = ContextCompat.checkSelfPermission(MainActivity.this, reqPermissions[0]) ==
                        PackageManager.PERMISSION_GRANTED;
                boolean permissionCheck2 = ContextCompat.checkSelfPermission(MainActivity.this, reqPermissions[1]) ==
                        PackageManager.PERMISSION_GRANTED;

                if (!(permissionCheck1 && permissionCheck2)) {
                    // If permissions are not already granted, request permission from the user.
                    ActivityCompat.requestPermissions(MainActivity.this, reqPermissions, requestCode);
                } else {
                    // Report other unknown failure types to the user - for example, location services may not
                    // be enabled on the device.
                    String message = String.format("Error in DataSourceStatusChangedListener: %s", dataSourceStatusChangedEvent
                            .getSource().getLocationDataSource().getError().getMessage());
                    Toast.makeText(MainActivity.this, message, Toast.LENGTH_LONG).show();
                }
            }
        });

        // Add a listener that callbacks everytime there is a change in location
        mLocationDisplay.addLocationChangedListener(new LocationDisplay.LocationChangedListener() {
            @RequiresApi(api = Build.VERSION_CODES.N)
            @Override
            public void onLocationChanged(LocationDisplay.LocationChangedEvent locationChangedEvent) {

                // Get the current location of the gps
                final Point gpsLocation = new Point(mLocationDisplay.getLocation()
                        .getPosition()
                        .getX(),
                        mLocationDisplay.getLocation()
                                .getPosition()
                                .getY(),
                        SpatialReferences.getWgs84());
                locationUpdated(gpsLocation);
            }
        });
    }

    private Point processBuffer(PointCollection pointsBuffer) {
        double avgPointX = 0,
                avgPointY = 0;

        // For each point in the lookup list of recent points
        for (Point gpsLocation : pointsBuffer) {
            avgPointX += gpsLocation.getX();
            avgPointY += gpsLocation.getY();
        }

        // Get the average
        // location in the
        // buffer
        avgPointX /= pointsBuffer.size();
        avgPointY /= pointsBuffer.size();

        return new Point(avgPointX,
                avgPointY);
    }

    private void locationUpdated(Point gpsLocation) {

        // Add the point to avg lookup list buffer
        mPointsBuffer.add(gpsLocation);

        // If the stopwatch is greater than the update speed then process the buffer
        mUpdateTimer += mGPSDelta.getMS();
        if (mUpdateTimer > mUpdateSpeed) {
            mUpdateTimer -= mUpdateSpeed;

            if ( mPointsBuffer.size() > 0) {
                Point point = processBuffer(mPointsBuffer);
                GPSPulseEvent(point);
                //mGPSHistory.add(point);
                mPointsBuffer.clear();
            }
        }

        // Split the time
        mGPSDelta.splitTime();
    }

    private void GPSPulseEvent(Point point) {
        Toast.makeText(MainActivity.this, "x:" + point.getX() + " y:" + point.getY(), Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        // If request is cancelled, the result arrays are empty.
        if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            // Location permission was granted. This would have been triggered in response to failing to start the
            // LocationDisplay, so try starting this again.
            mLocationDisplay.startAsync();
        } else {
            // If permission was denied, show toast to inform user what was chosen. If LocationDisplay is started again,
            // request permission UX will be shown again, option should be shown to allow never showing the UX again.
            // Alternative would be to disable functionality so request is not shown again.
            Toast.makeText(MainActivity.this, "Permission denied", Toast
                    .LENGTH_SHORT).show();
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onPause() {
        mMapView.pause();
        super.onPause();
    }

    @Override
    protected void onResume() {
        super.onResume();
        mMapView.resume();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        mMapView.dispose();
    }
}
