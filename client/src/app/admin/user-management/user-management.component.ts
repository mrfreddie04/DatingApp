import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService ) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers()
  {
    this.adminService.getUsersWithRoles()
      .subscribe(users=>{
        this.users = users;
      });
  }

  openRolesModal(user: Partial<User>){
    const config = {
      class: "modal-dialog-centered",
      initialState: {
        user:user,
        roles: this.getRolesArray(user)
      }
    };

    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    
    this.bsModalRef.content.updateSelectedRoles.subscribe(values => {  
      const roles = values.filter(r=>r.checked).map(r=>r.name);    
      this.adminService.updateUserRoles(user.username, roles)
        .subscribe(roles=>{
          user.roles = roles;
        });
    });
  }

  private getRolesArray(user: Partial<User>) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles = [
        {name:"Admin",value:"Admin"},
        {name:"Moderator",value:"Moderator"},
        {name:"Member",value:"Member"}]

    availableRoles.forEach(role=>{
      roles.push({
        name: role.name,
        value: role.value,
        checked: userRoles.includes(role.value)
      });      
    });  
    
    return roles;
  }
}
