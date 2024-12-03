import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { LedgerComponent } from './ledger/ledger.component';
import { AuthGuard } from './auth.guard';
import {CreateLedgerComponent} from './create-ledger/create-ledger.component';

export const routes: Routes = [
    {
      path: 'ledgers',
      component: LedgerComponent,
      canActivate: [AuthGuard], // Protect this route
    },
    {
      path: 'login',
      component: LoginComponent,
    },
    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full',
    },
    {
      path: 'create-ledger',
      component: CreateLedgerComponent,
    }
  ];
