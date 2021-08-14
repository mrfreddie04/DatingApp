import { MembersService } from './../../_services/members.service';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  public member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[]=[];

  constructor(private memberService: MembersService, private route: ActivatedRoute) { 
  }

  ngOnInit(): void {
    this.loadMember();

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
  }

  getImages(): NgxGalleryImage[] {
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

  loadMember() {
    const username = this.route.snapshot.paramMap.get("username");
    this.memberService.getMember(username).subscribe((member)=>{
      this.member = member;
      this.galleryImages = this.getImages();
    });
  }
}
