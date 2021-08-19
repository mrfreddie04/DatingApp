import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Role } from 'src/app/_models/role';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  @Input() updateSelectedRoles = new EventEmitter();
  user: Partial<User>;
  roles: Role[];

  constructor(public bsModalRef: BsModalRef) {}

  ngOnInit(): void {
  }

  updateRoles(){
    this.updateSelectedRoles.emit(this.roles);
    this.bsModalRef.hide();
  }

}
