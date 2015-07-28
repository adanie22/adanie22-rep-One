# ===================================================================================
# Author: Marina Krynina, CSC
# Func: AddUserToLocalAdministrators
# Desc: Adds specified user to the Local admins group
#       dbInstance = SERVERNAME, e.g. DR2DBSAWS003W
#       username = DOMAIN\USER
# ===================================================================================
Function AddUserToLocalAdministrators([string]$serverName, [string]$username)
{
    if (([string]::IsNullOrEmpty($serverName)))
    {
        log "ERROR: serverName is null or empty"
        return
    }

    if (([string]::IsNullOrEmpty($username)))
    {
        log "ERROR: username is null or empty"
        return
    }

    Try
    {
        $domain,$account = $username -Split "\\"

        log "INFO: Adding user $username to the Local Admins group $serverName"
        ([ADSI]"WinNT://$serverName/Administrators,group").Add("WinNT://$domain/$account")
        log "INFO: Added user $username to the Local Admins group $serverName"
    }
    Catch 
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }        
}

# ===================================================================================
# Author: Marina Krynina, CSC
# Func: CreateSQLLogin
# Desc: Creates SQL Login for a domain user
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       username = DOMAIN\USER
# ===================================================================================
function CreateSQLLogin([string]$dbInstance, [string]$username)
{
    [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null 
    $sqlSrv = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $dbInstance

    log "INFO: Checking if $username login exists in $dbInstance"
    $login = $sqlSrv.Logins | where {$_.Name -eq $username}
    if($login -eq $null)
    {
        $login = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $sqlSrv, $username
        $login.LoginType = 'WindowsUser'
        $login.Create()
        log "INFO: Created $username login in $dbInstance"
    }
    else
    {
        log "INFO: $username login already exists in $dbInstance"
    }

    return $login
}

# ===================================================================================
# Author: Stiven Skoklevski, CSC
# Func: CreateLocalSQLLogin
# Desc: Creates a local SQL Login
#       dbInstance = SERVER\INSTANCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       username = USER
#       password = password12345
# ===================================================================================
function CreateLocalSQLLogin([string]$dbInstance, [string]$username, [string]$pwd)
{
    [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null 
    $sqlSrv = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $dbInstance

    log "INFO: Checking if $username login exists in $dbInstance"
    $login = $sqlSrv.Logins | where {$_.Name -eq $username}
    if($login -eq $null)
    {
        $login = New-Object -TypeName Microsoft.SqlServer.Management.Smo.Login -ArgumentList $sqlSrv, $username
        $login.LoginType = 'SqlLogin'
        $login.PasswordExpirationEnabled = $false
        $login.Create($pwd)
        log "INFO: Created $username login in $dbInstance"
    }
    else
    {
        log "INFO: $username login already exists in $dbInstance"
    }

    return $login
}

# ===================================================================================
# Author: Marina Krynina, CSC
# Func: AddSQlLoginToSQLRole
# Desc: Creates SQL Login for a domain user
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       username = DOMAIN\USER
# ===================================================================================

function AddSQlLoginToSQLRole ([string]$dbInstance, [string]$role, [Microsoft.SqlServer.Management.Smo.Login]$login)
{
    try
    {
        [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null 
        $sqlSrv = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $dbInstance

        log "INFO: Checking if role = $role exists in $dbInstance"
        $svrole = $sqlSrv.Roles | where {$_.Name -eq $role}
        if ($svrole -ne $null)
        {

            try
            {
                $login.AddToRole($role)
                $login.Alter()
                log "INFO: Login $login has been added to $role role in $dbInstance"
            }
            catch
            {
                $error[0] | format-list -force 
            }
        }
        else
        {
            log "WARNING: role = $role does not exist in $dbInstance"
        }
    }
    Catch 
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }        


}

# ===================================================================================
# Author: Marina Krynina, CSC
# Func: CreateDBUser
# Desc: Creates DB user
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       dbName = database name, e.g. msdb
#       login = SQL Login
# ===================================================================================
function CreateDBUser([string]$dbInstance, [string]$dbName, [Microsoft.SqlServer.Management.Smo.Login]$login)
{
    try
    {
       [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null 
        $sqlSrv = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $dbInstance

        ## Creating database user and assigning database role    
        $dbUserName = $login.Name

        log "INFO: Checking if $dbName database exists in $dbInstance"
        $database = $sqlSrv.Databases | where {$_.Name -eq $dbName}
        if ($database -eq $null)
        {
            log "WARNING: Database $database does not exist in $dbInstance"
        }
        else
        {
            log "INFO: Checking if $dbUserName user exists in $dbName"
            $dbUser = $database.Users | where {$_.Name -eq $dbUserName}
            if ($dbUser -eq $null)
            {
                $dbUser = New-Object -TypeName Microsoft.SqlServer.Management.Smo.User -ArgumentList $database, $dbUserName
                $dbUser.Login = $dbUserName
                $dbUser.Create()
                log "INFO: Created $dbUserName user in $dbName"
            }
            else
            {
                log "INFO: $dbUserName user already exists in $dbName"
            }
        }
    }
    Catch 
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }        

    return $dbUser
}

# ===================================================================================
# Author: Marina Krynina, CSC
# Func: CreateDBUser
# Desc: Creates DB user
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       dbName = database name, e.g. msdb
#       role = DB role, e.g. SQLAgentReaderRole 
#       dbUser = DB user
# ===================================================================================
function AddDBUserToDBRole([string]$dbInstance, [string]$dbName, [string]$dbRole, [Microsoft.SqlServer.Management.Smo.User]$dbUser)
{
    try
    {
        [System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO') | out-null 
        $sqlSrv = New-Object ('Microsoft.SqlServer.Management.Smo.Server') $dbInstance

        log "INFO: Checking if $dbName database exists in $dbInstance"
        $database = $sqlSrv.Databases | where {$_.Name -eq $dbName}
        if ($database -eq $null)
        {
            log "WARNING: Database $database does not exist in $dbInstance"
        }
        else
        {
            log "INFO: Checking if role = $dbRole exists in $dbName"
            $role = $database.Roles | where {$_.Name -eq $dbRole}
            if ($role -ne $null)
            {
                $role.AddMember($dbUser.Name)
                $role.Alter
                log "INFO: User $dbUser.Name has been added to $dbRole role in $dbName"
            }
            else
            {
                log "WARNING: DB role = $dbRole does not exist in $dbInstance"
            }
        }
    }
    Catch 
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }        


}


# ===================================================================================
# Author: Marina Krynina, CSC
# Func: AssignSQLRoleToDomainUser
# Desc: Creates SQL Login for a domain user and assigns SQL Role
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       username = DOMAIN\USER
#       sqlRoles = "dbcreator,sysadmin" comma-delimeterd string
# ===================================================================================
Function AssignSQLRoleToDomainUser([string]$dbInstance, [string]$username, [string]$sqlRoles)
{
    if (([string]::IsNullOrEmpty($dbInstance)))
    {
        log "ERROR: dbInstance is null or empty"
        return
    }

    if (([string]::IsNullOrEmpty($username)))
    {
        log "ERROR: username is null or empty"
        return
    }

    try
    {
        $login = CreateSQLLogin $dbInstance $username
        if ($login -eq $null)
        {
            log "WARNING: Login $username is null"
            return
        }

        if (([string]::IsNullOrEmpty($sqlRoles)))
        {
            log "WARNING: sqlRoles is null or empty. "
        }
        else
        {
            $sqlRoles.Split(",") | ForEach {
                [string]$role = [string]($_).Trim()
            
                # check if the role is SQL Role or SQL Agent Role
                if($role.ToLower().StartsWith("sqlagent"))
                {
                    log "INFO: Role = $role is SQl Agent Role"
                    $dbName = "msdb"
                    $dbUser = CreateDBUser $dbInstance $dbName $login

                    if ($dbUser -ne $null)
                    {
                        AddDBUserToDBRole $dbInstance $dbName $role $dbUser
                    }
                    else
                    {
                        log "WARNING: DB User $username is null"
                    }
                }
                else
                {
                    log "INFO: Role = $role is SQl Role"
                    AddSQlLoginToSQLRole $dbInstance $role $login
                }
            }
        }
    }
    catch
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }
}


# ===================================================================================
# Author:Stiven Skoklevski, CSC
# Func: AssignSQLRoleToLocalUser
# Desc: Creates SQL Login for a local user and assigns SQL Role
#       dbInstance = SERVER\INSATNCE_NAME, e.g DR2DBSAWS003W\SP_CONFIG
#       username = USER
#       sqlRoles = "dbcreator,sysadmin" comma-delimeterd string
# ===================================================================================
Function AssignSQLRoleToLocalUser([string]$dbInstance, [string]$username, [string]$sqlRoles, [string]$pwd)
{
    if (([string]::IsNullOrEmpty($dbInstance)))
    {
        log "ERROR: dbInstance is null or empty"
        return
    }

    if (([string]::IsNullOrEmpty($username)))
    {
        log "ERROR: username is null or empty"
        return
    }

    if (([string]::IsNullOrEmpty($pwd)))
    {
        log "ERROR: pwd is null or empty"
        return
    }

    try
    {
        $login = CreateLocalSQLLogin $dbInstance $username $pwd
        if ($login -eq $null)
        {
            log "WARNING: Login $username is null"
            return
        }

        if (([string]::IsNullOrEmpty($sqlRoles)))
        {
            log "WARNING: sqlRoles is null or empty. "
        }
        else
        {
            $sqlRoles.Split(",") | ForEach {
                [string]$role = [string]($_).Trim()
            
                # check if the role is SQL Role or SQL Agent Role
                if($role.ToLower().StartsWith("sqlagent"))
                {
                    log "INFO: Role = $role is SQl Agent Role"
                    $dbName = "msdb"
                    $dbUser = CreateDBUser $dbInstance $dbName $login

                    if ($dbUser -ne $null)
                    {
                        AddDBUserToDBRole $dbInstance $dbName $role $dbUser
                    }
                    else
                    {
                        log "WARNING: DB User $username is null"
                    }
                }
                else
                {
                    log "INFO: Role = $role is SQl Role"
                    AddSQlLoginToSQLRole $dbInstance $role $login
                }
            }
        }
    }
    catch
    {
        $ex = $_.Exception | format-list | Out-String
        log "ERROR: $ex"
    }
}

