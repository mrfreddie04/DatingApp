import { AccountService } from 'src/app/_services/account.service';
import { PresenceService } from './../../_services/presence.service';
import { element } from 'protractor';
import { Message } from './../../_models/message';
import { MessageService } from './../../_services/message.service';
import { MembersService } from './../../_services/members.service';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Photo } from 'src/app/_models/photo';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { User } from 'src/app/_models/user';


@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild("memberTabs", {static: true}) memberTabs: TabsetComponent;
  public activeTab: TabDirective;
  public member: Member;
  public user: User;
  public messages: Message[] = [];
  public galleryOptions: NgxGalleryOptions[];
  public galleryImages: NgxGalleryImage[]=[];
  public onlineUser$: Observable<boolean>;

  constructor(
    private presenceService: PresenceService, 
    private accountService: AccountService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService) 
  { 
    this.accountService.currentUser$
      .pipe(take(1))
      .subscribe((user)=>this.user=user);

    this.router.routeReuseStrategy.shouldReuseRoute =  () => false; 
    
    this.onlineUser$ = this.presenceService.onlineUsers$.pipe(
      map( users => {
        return users.includes(this.member.username);
      })
    )      
  }

  ngOnInit(): void {
    //this.loadMember();
    this.route.data.subscribe((data)=>{
      this.member = data.member;
    });

    this.route.queryParams.subscribe((params: Params) => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        thumbnailsColumns: 4,
        imagePercent: 100,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];   

    this.galleryImages = this.getImages();
  }

  private getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    this.member.photos.forEach((photo: Photo)=>{
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      });
    });    
    return imageUrls;
  }

  // public loadMember() {
  //   const username = this.route.snapshot.paramMap.get("username");
  //   this.memberService.getMember(username).subscribe((member)=>{
  //     this.member = member;
  //     this.galleryImages = this.getImages();
  //   });
  // }

  public onTabActivated(data: TabDirective) {
    this.activeTab = data;
    //console.log(this.activeTab.heading)
    if( this.activeTab.heading === "Messages" && this.messages.length === 0) {
      this.messageService.createHubConnection( this.user, this.member.username);
      // this.messageService.messageThread$.subscribe((messages) => {
      //   this.messages = messages;
      // });
      //this.loadMessageThread();
    } else {
      this.messageService.stopHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  public selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  // private loadMessageThread() {
  //   console.log("Loading Message Thread...");
  //   this.messageService.getMessageThread(this.member.username).subscribe((response)=>{
  //     this.messages = response;
  //   });
  // }  
}
