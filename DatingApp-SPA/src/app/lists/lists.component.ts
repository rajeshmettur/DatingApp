import { Component, OnInit } from '@angular/core';
import { User } from '../_models/user';
import { Pagination, PaginatedResult } from '../_models/Pagination';
import { AuthService } from '../_services/auth.service';
import { UserServiceService } from '../_services/UserService.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyjsService } from '../_services/alertifyjs.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  user: User[];
  pagination: Pagination;
  likesParam: string;
  userParams: any = {};
  constructor(private authService: AuthService, private userService: UserServiceService,
              private route: ActivatedRoute, private alertify: AlertifyjsService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data.users.result;
      this.pagination = data.users.pagination;
    });
  }

  pageChanged(event: any): void {
    console.log(event.page);
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

loadUsers() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParam).
    subscribe((res: PaginatedResult<User[]>) => {
      console.log(this.likesParam);
      this.user = res.result;
      this.pagination = res.pagination;
    }, error => {
      this.alertify.error(error);
    });
}
}
