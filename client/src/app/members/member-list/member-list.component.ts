import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from './../../_services/members.service';
import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Observable } from 'rxjs';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { take } from 'rxjs/operators';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  public members: Member[] = [];
  public pagination: Pagination;
  public userParams: UserParams;
  public user: User;
  public genderList = [
    { value: "male", display: "Males"},
    { value: "female", display: "Females"}
  ];

  constructor(private membersService: MembersService) { 
    this.userParams = this.membersService.getUserParams();
    console.log("UP Component", this.userParams);
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  public pageChanged(event: any) {
    this.userParams.pageNumber = event.page;
    console.log("UP PC", this.userParams);
    this.loadMembers();
  }

  public resetFilters() {
    this.userParams = this.membersService.resetUserParams();
    this.loadMembers();
  }

  public loadMembers() {
    //console.log(this.userParams);
    this.membersService.setUserParams(this.userParams);
    this.membersService.getMembers(this.userParams).subscribe((paginatedResult)=>{
      this.members = paginatedResult.result;
      this.pagination = paginatedResult.pagination;
    });
  }
}
