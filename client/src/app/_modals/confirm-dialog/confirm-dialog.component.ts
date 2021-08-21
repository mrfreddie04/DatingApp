import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.css']
})
export class ConfirmDialogComponent implements OnInit {
  //@Input() confirmResult = new EventEmitter();
  title: string;
  message: string;
  btnOkText: string;
  btnCancelText: string;
  result: boolean;

  constructor(public bsModalRef: BsModalRef) {}

  ngOnInit(): void {
  }

  public confirm() {
    this.result = true;
    //this.confirmResult.emit(this.result);
    this.bsModalRef.hide();    
  }

  public decline() {
    this.result = false;
    //this.confirmResult.emit(this.result);
    this.bsModalRef.hide();    
  }  
}
