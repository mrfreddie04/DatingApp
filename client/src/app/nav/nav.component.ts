import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';
//import { User } from '../_models/user';
//import { Observable } from 'rxjs';
//import { Observable } from 'rxjs';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any={}; //to store form data
  //loggedIn: boolean;
  //currentUser$: Observable<User>;

  constructor(public accountService: AccountService, 
      //public memberService: MembersService, 
      private router: Router,
      private toastr: ToastrService) { };

  ngOnInit(): void {
    //this.getCurrentUser()
    //this.currentUser$ = this.accountService.currentUser$;
  }

  //method
  login(){
    this.accountService.login(this.model)
      .subscribe(res=>{
        //console.log(res);
        //this.memberService.setUser();
        this.router.navigateByUrl("/members");
      });
  };        

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl("/");
    //this.loggedIn = false;    
  };

  // getCurrentUser(){
  //   this.accountService.currentUser$.subscribe(user=>{
  //     //this.loggedIn = !!user; //!! turrn object into boolean null=>false,
  //   }, error => {
  //     console.log(error);
  //   });
  // }
}
