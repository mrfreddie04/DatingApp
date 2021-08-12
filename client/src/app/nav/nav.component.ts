import { AccountService } from './../_services/account.service';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { User } from '../_models/user';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  //loggedIn: boolean = false;
  currentUser$: Observable<User>

  constructor(
    private accountService: AccountService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.currentUser$ =  this.accountService.currentUser$;
    //this.getCurrentUser(); 
  }

  login() {
    this.accountService.login(this.model).subscribe(
      (response)=>{
        this.router.navigateByUrl("/members");
        console.log(response);
      },
      (err) => {
        this.toastr.error(err.error);
        console.error(err.error);
      }
    );
  }

  logout() {
    this.router.navigateByUrl("/");
    this.accountService.logout();
  }

  // getCurrentUser() {
  //   this.currentUser$ =  this.accountService.currentUser$;
  //   this.accountService.currentUser$.subscribe( 
  //     (user) => {
  //       this.loggedIn = !!user;
  //     },
  //     (err) => {
  //       console.error(err);
  //     }
  //   )
  // }
}
