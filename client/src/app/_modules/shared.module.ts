import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { BsDatepickerModule } from "ngx-bootstrap/datepicker";
import { ToastrModule } from 'ngx-toastr';
import { NgxGalleryModule } from '@kolkov/ngx-gallery';
import { FileUploadModule } from 'ng2-file-upload';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    NgxGalleryModule,
    FileUploadModule,
    BsDatepickerModule.forRoot(),
    BsDropdownModule.forRoot(),
    TabsModule.forRoot(),
    ToastrModule.forRoot({
      positionClass: "toast-bottom-right"
    })     
  ],
  exports:[
    BsDropdownModule,
    ToastrModule,
    TabsModule,
    NgxGalleryModule,
    FileUploadModule,
    BsDatepickerModule
  ]
})
export class SharedModule { }
