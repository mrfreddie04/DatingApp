import { MembersService } from './../../_services/members.service';
import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  public members$: Observable<Member[]>;

  constructor(private membersService: MembersService) { }

  ngOnInit(): void {
    this.members$ = this.membersService.getMembers();
    //this.loadMembers();
  }

  // private loadMembers() {
  //   this.membersService.getMembers().subscribe((members) => {
  //     this.members = members;
  //   });
  // }
}
