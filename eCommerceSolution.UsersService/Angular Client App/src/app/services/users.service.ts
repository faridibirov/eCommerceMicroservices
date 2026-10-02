import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthenticationResponse } from '../models/authentication-response';
import { Register } from '../models/register';
import { environment } from '../../environment';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private baseUrl: string = environment.apiUrl;
  public isAuthenticated: boolean = false;
  public currentUserName: string | null = "";

  constructor(private http: HttpClient) {
    // Check local storage for authentication status on application startup
    this.isAuthenticated = !!localStorage.getItem('authToken');
    this.currentUserName = localStorage.getItem("currentUserName");
  }

  register(register: Register): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.baseUrl}register`, register);
  }

  login(email: string, password: string): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.baseUrl}login`, { email, password });
  }

  setAuthStatus(token: string, currentUserName: string): void {
    this.isAuthenticated = true;
    localStorage.setItem('authToken', token);
    localStorage.setItem('currentUserName', currentUserName);
    this.currentUserName = currentUserName;
  }

  logout(): void {
    this.isAuthenticated = false;
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUserName');
  }
}

//Run: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
