import { Routes } from '@angular/router';
import { RegisterComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { ShowCaseComponent } from './products/show-case/show-case.component';

export const routes: Routes = [
    { path: 'auth/register', component: RegisterComponent },
    { path: 'auth/login', component: LoginComponent },
    { path: 'products/showcase', component: ShowCaseComponent },
    { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
];
