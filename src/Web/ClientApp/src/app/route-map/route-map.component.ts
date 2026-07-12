import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

declare const google: any;

interface RouteStopInfo {
  address: string;
  estimatedArrival: string | null;
}

interface RouteMapDto {
  navigationUrl: string;
  stops: RouteStopInfo[];
}

@Component({
  selector: 'app-route-map',
  templateUrl: './route-map.component.html'
})
export class RouteMapComponent implements OnInit, OnDestroy {
  routeId!: number;
  routeData: RouteMapDto | null = null;
  loading = true;
  error = '';
  private map: any;
  private directionsService: any;
  private directionsRenderer: any;

  constructor(private http: HttpClient, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.routeId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadRoute();
  }

  loadRoute(): void {
    this.http.get<RouteMapDto>(`/api/Routes/${this.routeId}/map`).subscribe({
      next: (data) => {
        this.routeData = data;
        this.loading = false;
        setTimeout(() => this.initMap(), 100);
      },
      error: () => {
        this.error = 'Failed to load route.';
        this.loading = false;
      }
    });
  }

  initMap(): void {
    if (!this.routeData || this.routeData.stops.length === 0) return;

    this.map = new google.maps.Map(document.getElementById('map'), {
      zoom: 12,
      center: { lat: 32.0853, lng: 34.7818 } // Tel Aviv default
    });

    this.directionsService = new google.maps.DirectionsService();
    this.directionsRenderer = new google.maps.DirectionsRenderer({ map: this.map });

    const stops = this.routeData.stops;
    const origin = stops[0].address;
    const destination = stops[stops.length - 1].address;
    const waypoints = stops.slice(1, -1).map(s => ({
      location: s.address,
      stopover: true
    }));

    this.directionsService.route({
      origin,
      destination,
      waypoints,
      travelMode: google.maps.TravelMode.DRIVING,
      optimizeWaypoints: false
    }, (result: any, status: any) => {
      if (status === 'OK') {
        this.directionsRenderer.setDirections(result);
      }
    });

    // Add numbered markers for each stop
    stops.forEach((stop, index) => {
      const geocoder = new google.maps.Geocoder();
      geocoder.geocode({ address: stop.address }, (results: any, status: any) => {
        if (status === 'OK') {
          new google.maps.Marker({
            position: results[0].geometry.location,
            map: this.map,
            label: `${index + 1}`,
            title: stop.address
          });
        }
      });
    });
  }

  openNavigation(): void {
    if (this.routeData) window.open(this.routeData.navigationUrl, '_blank');
  }

  ngOnDestroy(): void {
    this.directionsRenderer?.setMap(null);
  }
}
