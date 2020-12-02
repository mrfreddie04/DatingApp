import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';

import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild("memberTabs", {static:true}) memberTabs: TabsetComponent; 
  activeTab: TabDirective;
  member: Member;
  user: User;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  messages: Message[] = []; //moved over from member-messages component

  constructor(public presence: PresenceService, private route: ActivatedRoute,
      private messageService: MessageService, private accountService: AccountService,
      private router: Router) { 
        this.accountService.currentUser$.pipe(take(1)).subscribe(user=>this.user = user);
        this.router.routeReuseStrategy.shouldReuseRoute = () => false;
      }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    //this.loadMember();
    this.route.data.subscribe(data=>{
      this.member = data.member;
      this.galleryImages = this.getImages();      
    });

    this.route.queryParams.subscribe(params=>{
      //check if we have sth in params.tab
      //if yes - navigate to this tab, otherwise select the first tab
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    });

    this.galleryOptions = [{
        width:"500px",
        height:"500px",
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
     }];      
  }

  getImages() : NgxGalleryImage[]{
    const imageUrls : NgxGalleryImage[] = [];

    this.member.photos.forEach(image=>{
      imageUrls.push({
                        small: image?.url,
                        medium: image?.url,
                        big: image?.url
                      });
    });

    return imageUrls;
  }

  // loadMember(){
  //   const username: string = this.route.snapshot.paramMap.get("username");    
  //   this.memberService.getMember(username)
  //     .subscribe(member=>{
  //       this.member = member;
  //       this.galleryImages = this.getImages();
  //     });
  // }  

  loadMessages()
  {
    this.messageService.getMessageThread(this.member.username)
      .subscribe(result=>{
        this.messages = result;
      });
  }  

  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading=="Messages"){
      //this.loadMessages();
      if(this.messages.length==0)
        this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  selectTab(tabId: number){
    this.memberTabs.tabs[tabId].active = true;
  }
}
