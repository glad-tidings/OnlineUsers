# Online Users
Online Users component for ASP.NET

## Example
```c#
ParsElecom.OnlineUsers.OnlineUsers onlineUsers = new ParsElecom.OnlineUsers.OnlineUsers();
onlineUsers.SetUserOnline("user1");
onlineUsers.SetUserOnline("user2");
onlineUsers.SetUserOnline("user3");

Console.WriteLine(onlineUsers.RegistredUsersCount);
Console.WriteLine(onlineUsers.GuestUsersCount);

onlineUsers.SetUserOffline("user2");

Console.WriteLine(onlineUsers.RegistredUsersCount);
Console.WriteLine(onlineUsers.GuestUsersCount);
```



![](http://visit.parselecom.com/Api/Visit/glad-tidings/OnlineUsers/F80000)
