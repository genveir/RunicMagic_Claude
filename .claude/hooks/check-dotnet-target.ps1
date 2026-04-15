# Blocks dotnet build/test commands that target paths outside the RunicMagic_Claude workspace.
# Commands with no absolute path are always allowed (they run against the workspace by default).

$data = [Console]::In.ReadToEnd() | ConvertFrom-Json
$cmd  = $data.tool_input.command

if ($cmd -notmatch '^dotnet (build|test)') { exit 0 }
if ($cmd -notmatch '[A-Za-z]:[/\\]')       { exit 0 }
if ($cmd -match    'E:[/\\]repos[/\\]RunicMagic_Claude') { exit 0 }

@{
    hookSpecificOutput = @{
        hookEventName          = "PreToolUse"
        permissionDecision     = "deny"
        permissionDecisionReason = "dotnet build/test is only permitted within the RunicMagic_Claude workspace"
    }
} | ConvertTo-Json -Depth 5

exit 2
