import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environment';
import { Claim } from '../models/claim';
import { MsalService } from '@azure/msal-angular';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private usersAPIURL: string = environment.usersAPIURL;

  public isAuthenticated: boolean = false;
  public claims: Claim[] = [];

  public isAdmin: boolean = false;
  public adminUserId: string = '7654da76-f945-4e20-ba67-aba7f6736d7e';
  public currentUserName: string = '';
  public currentUserId: string = '';

  constructor(private http: HttpClient) {}

  getClaim(claimName: string): Claim {
    return this.claims.find((temp) => temp.claim == claimName) as Claim;
  }

  setClaims(claims: Claim[]) {
    this.claims = claims;
  }

  setIsAuthenticated(authService: MsalService) {
    const account =
      authService.instance.getActiveAccount() ??
      authService.instance.getAllAccounts()[0];

    this.isAuthenticated = !!account;

    if (!account) {
      this.isAdmin = false;
      this.currentUserId = '';
      this.currentUserName = '';
      return;
    }

    const oid = account.idTokenClaims?.oid;

    if (!oid) {
      console.error('OID claim not found in ID token');
      return;
    }

    this.currentUserId = oid;
    this.isAdmin = oid === this.adminUserId;

    this.currentUserName =
      (account.idTokenClaims?.['given_name'] as string | undefined) ??
      account.name ??
      '';
  }

  getAuthToken(): any {
    const token: any = localStorage.getItem('authToken');
    return token;
  }
}
