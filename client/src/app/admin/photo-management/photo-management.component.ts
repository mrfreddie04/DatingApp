import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';
import { ConfirmService } from 'src/app/_services/confirm.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: Partial<Photo[]>;

  constructor(private adminService: AdminService, private confirmService: ConfirmService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval(){
    this.adminService.getPhotosForApproval().subscribe(photos=>{
      this.photos = photos;
    });
  }

  approvePhoto(photoId: number){    
    this.adminService.approvePhoto(photoId)
    .subscribe(()=>{
      //this.messages = this.messages.filter(item=>item.id != id);
      this.photos.splice(this.photos.findIndex(p=>p.id == photoId),1);
    });        
  }

  rejectPhoto(photoId: number){
    this.confirmService.confirm("Confirm message rejection", "This cannot be undone")
      .subscribe(result=>{
        if(result)
        {
          //OK to delete
          this.adminService.rejectPhoto(photoId)
          .subscribe(()=>{
            //this.messages = this.messages.filter(item=>item.id != id);
            this.photos.splice(this.photos.findIndex(p=>p.id == photoId),1);
          });          
        }
      });    
  }  

}
