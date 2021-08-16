import { AccountService } from './../_services/account.service';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { User } from '../_models/user';
import { MembersService } from '../_services/members.service';


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
    private memberService: MembersService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.currentUser$ =  this.accountService.currentUser$;
    //this.getCurrentUser(); 
  }

  login() {
    this.accountService.login(this.model).subscribe(
      (user: User)=>{
        this.memberService.resetUserParamsLogin(user);
        this.router.navigateByUrl("/members");
        console.log(user);
      },
      (err) => {
        //this.toastr.error(err.error);
        console.error(err.error);
      }
    );
  }

  logout() {
    this.router.navigateByUrl("/");
    this.model.username = "";
    this.model.password = "";
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
