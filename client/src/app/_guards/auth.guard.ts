import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { AccountService} from '../_services/account.service';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private accountService: AccountService,
    private toastr: ToastrService){}

  //possible return types
  canActivate(): Observable<boolean> 
  {
    return this.accountService.currentUser$
      .pipe(map((user:User)=>{
        if(user)
          return true;

        this.toastr.error("You shall not pass!");
        return false;
      }));
  }
  
}
