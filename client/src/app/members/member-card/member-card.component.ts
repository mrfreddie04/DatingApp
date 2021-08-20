import { PresenceService } from './../../_services/presence.service';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from './../../_services/members.service';
import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() member: Member;
  public onlineUser$: Observable<boolean>;
  
  constructor(
    private membersService: MembersService,
    private presenceService: PresenceService,
    private toastr: ToastrService
  ) { 
    this.onlineUser$ = this.presenceService.onlineUsers$.pipe(
      map( users => {
        return users.includes(this.member.username);
      })
    )
  }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    this.membersService.addLike(member.username).subscribe(()=>{
      this.toastr.success(`You have liked ${member.knownAs}`);
    });
  }
}
