import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';

import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  member: Member;

  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadMember();

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

  loadMember(){
    const username: string = this.route.snapshot.paramMap.get("username");
    this.memberService.getMember(username)
      .subscribe(member=>{
        this.member = member;
        this.galleryImages = this.getImages();
      });
  }  
}
