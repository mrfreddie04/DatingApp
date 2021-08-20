import { AdminService } from './../../_services/admin.service';
import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/user';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/_modals/roles-modal/roles-modal.component';
import { Role } from 'src/app/_models/role';
import { abort } from 'process';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  public users: Partial<User>[];
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.loadUsersWithRoles();
  }

  private loadUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe( (users) => {
      this.users = users;
    });
  }

  public openRolesModal(user: Partial<User>) {
    const config = {
      class: "modal-dialog-centered",
      initialState: {
        user: user,
        roles: this.getRolesArray(user)
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe( (values: Role[]) => {      
      console.log("Roles returned by modal",values);
      const rolesToUpdate = values.filter( value => value.checked).map( value => value.name);
      console.log("Roles to update",rolesToUpdate);
      if(rolesToUpdate && rolesToUpdate.length>0) {
        this.adminService.updateUserRoles(user.username, rolesToUpdate.join(",")).subscribe((roles: string[])=>{
          console.log("Roles after update",roles);
          user.roles = [...roles];  
        })
      }
    })
  }

  private getRolesArray(user: Partial<User>) {
    const roles: Role[] =[];
    const availableRoles: Role[] = [
      {name: "Admin", value: "Admin", checked: false},
      {name: "Moderator", value: "Moderator", checked: false},
      {name: "Member", value: "Member", checked: false}
    ]; 

    availableRoles.forEach( role => {
      role.checked = user.roles.includes(role.name);
      roles.push(role);
    })

    return roles;
  }
}

