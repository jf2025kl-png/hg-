dotnet add package Pomelo.EntityFrameworkCore.MySql
3. 添加迁移
在项目目录运行：

bash
dotnet ef migrations add Init
这会在项目里生成一个 Migrations 文件夹，里面包含建表的 SQL 脚本。

4. 更新数据库
运行：

bash
dotnet ef database update
EF Core 会自动把迁移脚本应用到 MySQL 数据库，生成对应的表。