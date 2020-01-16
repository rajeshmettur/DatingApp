import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserServiceService } from '../../_services/UserService.service';
import { AlertifyjsService } from '../../_services/alertifyjs.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/Pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  user: User = JSON.parse(localStorage.getItem('user'));
  pagination: Pagination;
  genderList = [{value: 'male', VRDisplay: 'Males'}, {value: 'female', VRDisplay: 'Females'}];
  userParams: any = {};

  constructor(private userService: UserServiceService, private alertify: AlertifyjsService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data.users.result;
      this.pagination = data.users.pagination;
    });
    this.userParams.gender = this.userParams.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 75;
    this.userParams.orderBy = 'lastActive';
  }

  pageChanged(event: any): void {
    console.log(event.page);
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }
   resetFilters() {
    this.userParams.gender = this.userParams.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 75;
    this.userParams.orderBy = 'lastActive';
  }
loadUsers() {
      this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams).
      subscribe((res: PaginatedResult<User[]>) => {
        this.users = res.result;
        this.pagination = res.pagination;
      }, error => {
        this.alertify.error(error);
      });
  }
}
