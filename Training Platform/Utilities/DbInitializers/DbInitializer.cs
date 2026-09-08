using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Training_Platform.Utilities.DbInitializers;

namespace Training_Platform.Utilities.DbInitailzers
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(ApplicationDbContext context, RoleManager<IdentityRole<int>> roleManager,
            UserManager<ApplicationUser> userManager , IConfiguration configuration,
            ILogger<DbInitializer> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Initialize()
        {
            try
            {
                //// 1- Update date Base
                //if (_context.Database.GetPendingMigrations().Any())
                //{
                //    _context.Database.Migrate();
                //}
                // 2- Create Roles
                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    await _roleManager.CreateAsync(new(RoleNames.SUPER_ADMIN));

                    await _roleManager.CreateAsync(new(RoleNames.TRAINER));

                    await _roleManager.CreateAsync(new(RoleNames.TRAINEE));
                }

                // 3- Create Admin User
                if (await _userManager.FindByEmailAsync(_configuration["SuperAdminAccount:Email"]!) is null)
                {
                    var admin = new ApplicationUser
                    {
                        FirstName = "Super",
                        LastName = "Admin",
                        UserName = _configuration["SuperAdminAccount:UserName"],
                        Email = _configuration["SuperAdminAccount:Email"],
                        EmailConfirmed = true,
                    };
                    await _userManager.CreateAsync(admin, _configuration["SuperAdminAccount:Password"]!);
                    await _userManager.AddToRoleAsync(admin, RoleNames.SUPER_ADMIN);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
           
        


    }
}