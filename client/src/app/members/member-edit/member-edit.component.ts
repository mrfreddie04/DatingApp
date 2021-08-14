import { ToastrService } from 'ngx-toastr';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from './../../_services/members.service';
import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { switchMap, take } from 'rxjs/operators';
import { User } from 'src/app/_models/user';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild("editForm") editForm: NgForm;
  @HostListener("window:beforeunload", ["$event"]) unloadNotification($event: any) {
    if(this.editForm.dirty) {
      $event.returnValue = true;
    }
  };

  public member: Member;
  public user: User;

  constructor(
    private accountService: AccountService,
    private memberService: MembersService,
    private toastr: ToastrService
  ) { 
    this.loadMember();
  }

  ngOnInit(): void {
  }

  public updateMember() {
    this.memberService.updateMember(this.member).subscribe(()=>{
      this.toastr.success("Profile updatedd successfully");
      this.editForm.reset(this.member);
    });
  }  

  private loadMember() {
    this.accountService.currentUser$.pipe(
      take(1),
      switchMap((user: User)=>{
        this.user = user;
        return this.memberService.getMember(user.username)
      })
    ).subscribe((member: Member)=>{
      this.member = member;
    });  
  }

}
