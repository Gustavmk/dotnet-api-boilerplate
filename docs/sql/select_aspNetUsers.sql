SELECT "Id",
       "UserName",
       "NormalizedUserName",
       "Email",
       "NormalizedEmail",
       "EmailConfirmed",
       "PasswordHash",
       "SecurityStamp",
       "ConcurrencyStamp",
       "PhoneNumber",
       "PhoneNumberConfirmed",
       "TwoFactorEnabled",
       "LockoutEnd",
       "LockoutEnabled",
       "AccessFailedCount"
FROM public."AspNetUsers"
LIMIT 1000;