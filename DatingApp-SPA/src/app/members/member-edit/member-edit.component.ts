import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute } from '@angular/router';
import { AlertifyjsService } from 'src/app/_services/alertifyjs.service';
import { NgForm } from '@angular/forms';
import { AuthService } from 'src/app/_services/auth.service';
import { UserServiceService } from 'src/app/_services/UserService.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
 constructor(private route: ActivatedRoute, private alertify: AlertifyjsService,
             private userService: UserServiceService, private authService: AuthService) { }
  @ViewChild('editForm', {static: true}) editForm: NgForm;

 user: User;
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data.user;
    });
  }

  updateUser() {
    this.userService.updateUser(this.authService.decodedToken.nameid, this.user).subscribe(next => {
      this.alertify.success('Profile updated successfully ');
      this.editForm.reset(this.user);
    }, error => {
      console.log(error);
      this.alertify.error(error);
    });
  }
}
