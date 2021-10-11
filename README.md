# project description

###Goal:
In the project, an MVP of a project management system is to be realized, which makes it possible to create projects and delegate task packages to employees.

###Brief description:
Within the project a project management system (MVP) was developed. The system offered the possibility to create projects and to add tasks or subtasks to them. The created tasks and subtasks could be assigned to employees.
In addition, there was a simple user administration.

The individual user roles to be assumed (administrator, project owner, project participant) are described in more detail below. In particular, the rights regarding the use of the 
rights regarding the use of the "PM System" website. The users get the 
possibility, depending on the assigned rights, to create and edit projects, to assign users to projects 
and remove them from projects, create, view, edit and delete project tasks. 
edit and delete project tasks. 
## User roles
A user in the role of an administrator (level 3) has access to the full functional 
functional range of the PM system. He is the organizational/administrative instance, which deals in 
in particular with the user administration. He is responsible for creating new users 
roles (project owner or project participant) and rights, as well as users who are no longer required within the PM system, 
users that are no longer needed within the PM system. Furthermore 
create, edit and delete projects. 
A user in the role of the project owner (level 2) has access to all the projects assigned to him and has all the rights and functions of a project owner. 
projects assigned to him and has all rights and functions of a project participant. Within the projects 
assigned projects, he can manage the rights for the project participants assigned to the project, as well as 
as well as create new (partial) tasks or delete those that are no longer required. Thereby he can 
assign individual subtasks to project participants or delete their rights for the project. 
delete them. In addition, subtasks can be assigned to him for processing. Thereby 
the tasks do not necessarily have to be in a project managed by him. The 
Furthermore, he can transfer the rights of the projects assigned to him to other users. 
A user in the role of a project participant (level 1) has the most limited rights within the PM system. 
most limited rights. A project participant is assigned either by the administrator or 
a project owner to a subtask. After processing a subtask he can 
the status of a subtask to completed, which updates the progress of the project. 
is updated. 

## How to run the application
1. Download visual studio (at least version 2019)
2. Open the project and configure the Database connection ( the so-call "connectionStrings" which is to find at the end of the file "Webconfig" present in the root folder)
3. Run the query in the file "add_Admin.sql" to add an "admin user".
4. Run the IIS Express and enjoy
