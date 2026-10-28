using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Services;

public sealed class WorktreeStatusService
{
    private readonly GitService _git;

    public WorktreeStatusService(GitService git)
    {
        _git = git;
    }

    public List<WorktreeDisplayItem> BuildDisplayItems(IEnumerable<GitWorktree> worktrees)
    {
        return worktrees.Select(GetStatus).ToList();
    }

    public WorktreeDisplayItem GetStatus(GitWorktree worktree)
    {
        var statusOutput = GetStatusOutputSafe(worktree.Path);

        var changedFiles = statusOutput
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        return new WorktreeDisplayItem
        {
            Worktree = worktree,
            HasChanges = changedFiles.Count > 0,
            ChangedFiles = changedFiles,
            LastModified = GetLastModified(worktree.Path)
        };
    }

    private string GetStatusOutputSafe(string worktreePath)
    {
        if (!Directory.Exists(worktreePath))
            return "";

        if (!IsGitLinked(worktreePath))
            return "";

        try
        {
            return _git.GetStatusPorcelain(worktreePath);
        }
        catch
        {
            return "";
        }
    }

    private static bool IsGitLinked(string worktreePath)
    {
        var gitPath = Path.Combine(worktreePath, ".git");
        return File.Exists(gitPath) || Directory.Exists(gitPath);
    }

    private static DateTime? GetLastModified(string path)
    {
        if (!Directory.Exists(path))
            return null;

        try
        {
            return Directory.GetLastWriteTime(path);
        }
        catch
        {
            return null;
        }
    }
}
