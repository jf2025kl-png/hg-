using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
namespace 皇冠娱乐
{
    //Add-Migration Init
    //Update-Database
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DataContext()
        {
        }

        /// <summary>
        /// 平台集合
        /// </summary>
        public DbSet<Platform> Platforms { get; set; }
        /// <summary>
        /// 合伙人集合
        /// </summary>
        public DbSet<Partner> Partners { get; set; }
        /// <summary>
        /// 平台的操作记录
        /// </summary>
        public DbSet<PlatformOperateHistory> PlatformOperateHistorys { get; set; }
        /// <summary>
        /// 平台财务记录
        /// </summary>
        public DbSet<PlatformFinanceHistory> PlatformFinanceHistorys { get; set; }
        /// <summary>
        /// 平台玩家集合
        /// </summary>
        public DbSet<Player> Players { get; set; }
        /// <summary>
        /// 玩家财务记录
        /// </summary>
        public DbSet<PlayerFinanceHistory> PlayerFinanceHistorys { get; set; }
        /// <summary>
        /// 红包集合
        /// </summary>
        public DbSet<Game> Games { get; set; }
        /// <summary>
        /// 红包历史记录
        /// </summary>
        public DbSet<GameHistory> GameHistorys { get; set; }

    
        /// <summary>
        /// 存储聊天对话
        /// </summary>
        public DbSet<BotChat> BotChats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Platform>().HasKey(e => e.CreatorId);
            modelBuilder.Entity<BotChat>().HasKey(e => e.Id);
            modelBuilder.Entity<Partner>().HasKey(e => e.Id);
            modelBuilder.Entity<PlatformOperateHistory>().HasKey(e => e.Id);
            modelBuilder.Entity<PlatformFinanceHistory>().HasKey(e => e.Id);
            modelBuilder.Entity<Player>().HasKey(e => e.PlayerId);
            modelBuilder.Entity<PlayerFinanceHistory>().HasKey(e => e.Id);
            modelBuilder.Entity<Game>().HasKey(e => e.Id);
            modelBuilder.Entity<GameHistory>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($"server=DESKTOP-DTQ3RU9;Database=Casino;Persist Security Info=True;User ID=sa;password=XiaoYanYan88;TrustServerCertificate=true;MultipleActiveResultSets=true");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
