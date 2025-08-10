# Unity Project Git Flow Convention - Complete Guide

## 🚀 Objectives
- Manage Unity project using standard Git Flow with Jira integration
- Avoid scene and `.meta` file conflicts
- Keep code and assets organized and manageable
- Ensure stable and scalable development workflow
- Track development progress through Jira tickets
- Guide from theory to practical implementation

---

## 📂 Branch Structure

### 🌿 Main Branches
- **`master`** → Stable code, tested, ready for build and deploy
- **`develop`** → Development code, integrating features from feature branches

### 🌱 Supporting Branches
- **`feature/*`** → Branches for new features
- **`release/*`** → Release preparation, only minor bug fixes and polish
- **`hotfix/*`** → Emergency bug fixes from `master`
- **`support/*`** → Support for older versions

---

## 🛠 Git Flow Setup

```bash
# Already configured with command:
git flow init -d

# Default configuration:
# - Production branch: master
# - Development branch: develop
# - Feature prefix: feature/
# - Release prefix: release/
# - Hotfix prefix: hotfix/
# - Support prefix: support/
```

---

## 🔗 Jira Workflow Integration

### 📋 Jira Ticket Management
- **Epic:** Large features broken into multiple stories
- **Story:** Individual user stories (PROJ-123, PROJ-124, etc.)
- **Bug:** Bug reports (PROJ-456, PROJ-457, etc.)
- **Task:** Development tasks (PROJ-789, PROJ-790, etc.)

### 🔄 Jira Status Workflow
1. **To Do** → Ticket created, ready for development
2. **In Progress** → Developer starts working (create feature branch)
3. **In Review** → Code review phase (create pull request)
4. **Done** → Feature complete, merged to develop
5. **Closed** → Released to production

### 📝 Jira Ticket Best Practices
- Always include acceptance criteria in ticket description
- Add screenshots/mockups for UI changes
- Link related tickets (blocks, relates to)
- Update time tracking in Jira
- Add commit hashes to ticket comments for traceability

---

## 🏷️ Git Tag Guidelines

### 📅 When to Use Tags

**1. 🚀 Release Tags (Required)**
```bash
# When completing a release
git flow release finish 1.0.0
# Automatically creates tag: v1.0.0

# Or create tag manually
git tag -a v1.0.0 -m "Release version 1.0.0 - User Authentication System"
git push origin v1.0.0
```

**2. 🔥 Hotfix Tags (Required)**
```bash
# When completing a hotfix
git flow hotfix finish PROJ-999-critical-fix
# Automatically creates tag: v1.0.1

# Or create tag manually
git tag -a v1.0.1 -m "Hotfix v1.0.1 - Fix critical game crash"
git push origin v1.0.1
```

**3. 🎯 Milestone Tags (Optional)**
```bash
# Mark important milestones
git tag -a milestone-alpha -m "Alpha version completed"
git tag -a milestone-beta -m "Beta version completed"
git push origin milestone-alpha
```

### 🏷️ Tag Naming Convention

**Release Tags:**
- `v1.0.0` - Major release
- `v1.1.0` - Minor release  
- `v1.0.1` - Patch/hotfix
- `v2.0.0` - Major version update

**Hotfix Tags:**
- `v1.0.1` - Hotfix for v1.0.0
- `v1.0.2` - Next hotfix
- `v1.1.1` - Hotfix for v1.1.0

**Special Tags:**
- `milestone-alpha` - Alpha milestone
- `milestone-beta` - Beta milestone
- `demo-v1` - Demo version

### 📋 Tag Workflow with Jira

**1. Create Release Tag:**
```bash
# In Jira: Create release version "v1.0.0"
# In Git: 
git flow release finish 1.0.0
git push origin v1.0.0

# Update Jira release with git tag link
```

**2. Create Hotfix Tag:**
```bash
# In Jira: Create urgent bug ticket PROJ-999
# In Git:
git flow hotfix finish PROJ-999-critical-fix
git push origin v1.0.1

# Update Jira ticket with hotfix tag
```

**3. Link Tags with Jira:**
- Add git tag link to Jira release
- Comment commit hash in Jira ticket
- Update release notes with tag information

---

## 📋 Detailed Workflow Process

### Step 1: 🆕 Start New Feature

#### 1.1 Create Feature Branch
```bash
# Create feature branch from develop using Jira ticket ID
git flow feature start PROJ-123-feature-name

# Or with detailed description
git flow feature start PROJ-123-feature-name -d "JIRA-123: Add login system"

# Real example:
git flow feature start PROJ-123-player-movement
```

**Feature naming rules with Jira:**
- `feature/PROJ-123-user-authentication`
- `feature/PROJ-456-inventory-system`
- `feature/PROJ-789-level-1-design`
- `feature/PROJ-101-audio-integration`

**Jira Ticket Format:**
- Use project key (e.g., PROJ, GAME, UNITY) + ticket number
- Include brief descriptive name after ticket number
- Example: `PROJ-123-user-auth` for ticket PROJ-123

#### 1.2 Check Status
```bash
# Check current branch
git branch
# Output: * feature/PROJ-123-player-movement

# Check status
git status
# Output: On branch feature/PROJ-123-player-movement
#         Your branch is up to date with 'origin/develop'
```

#### 1.3 Update Jira
- Move ticket `PROJ-123` from "To Do" → "In Progress"
- Add comment: "Started development on feature branch"

---

### Step 2: 🔄 Develop Feature

#### 2.1 Work on Feature
```bash
# Create and edit files
# Example: Assets/Scripts/PlayerMovement.cs

# Commit frequently with clear messages including Jira ticket
git add Assets/Scripts/PlayerMovement.cs
git commit -m "feat(PROJ-123): add basic player movement script"

# Add input handling
git add Assets/Scripts/InputManager.cs
git commit -m "feat(PROJ-123): implement WASD input handling"

# Add animation
git add Assets/Animations/PlayerWalk.anim
git commit -m "feat(PROJ-123): add player walk animation"
```

**Commit Message Convention with Jira:**
- `feat(PROJ-123):` - New feature
- `fix(PROJ-456):` - Bug fix
- `docs(PROJ-789):` - Documentation update
- `style(PROJ-101):` - Code formatting changes
- `refactor(PROJ-202):` - Code refactoring
- `test(PROJ-303):` - Adding tests
- `chore(PROJ-404):` - Maintenance tasks

**Jira Integration:**
- Always include Jira ticket number in commit messages
- Use format: `type(PROJ-123): description`
- Link commits to Jira tickets for automatic tracking

#### 2.2 Push to Remote
```bash
# Push feature branch to remote
git push origin feature/PROJ-123-player-movement

# Result: Branch pushed to GitHub/GitLab
```

#### 2.3 Sync with Develop (if needed)
```bash
# If develop has new updates from other team members
git checkout develop
git pull origin develop
git checkout feature/PROJ-123-player-movement
git merge develop

# Resolve conflicts if any
# Then push again
git push origin feature/PROJ-123-player-movement
```

#### 2.4 Test Feature
```bash
# Test in Unity Editor
# - Check player movement works
# - Ensure no compile errors
# - Test scene is not broken
```

---

### Step 3: ✅ Finish Feature

#### 3.1 Final Check
```bash
# Check status
git status
# Ensure all changes are committed

# Check log
git log --oneline -5
# View last 5 commits
```

#### 3.2 Finish Feature
```bash
# Merge feature into develop
git flow feature finish PROJ-123-player-movement

# Result:
# - Feature merged into develop
# - Feature branch deleted
# - You're switched back to develop branch
# - Git shows merge summary
```

#### 3.3 Push Develop
```bash
# Push develop to remote
git push origin develop

# Result: Develop branch updated on remote
```

#### 3.4 Update Jira
- Move ticket `PROJ-123` from "In Progress" → "Done"
- Add comment with commit hash
- Link feature branch to ticket
- Update time tracking

---

### Step 4: 🚀 Prepare Release

#### 4.1 Decide to Create Release
```bash
# Only create release when:
# - Enough features for release
# - Code has been tested
# - Team agrees on release

# Create release branch from develop
git flow release start 1.0.0

# Result:
# - Created branch: release/1.0.0
# - Automatically checkout to release branch
# - Branch created from develop
```

#### 4.2 Work on Release
```bash
# ONLY fix minor bugs and polish, DO NOT add new features

# Example: Adjust movement speed
git add Assets/Scripts/PlayerMovement.cs
git commit -m "fix(PROJ-999): adjust player movement speed for better gameplay"

# Update version number
git add ProjectSettings/ProjectVersion.txt
git commit -m "chore(PROJ-999): update version to 1.0.0"

# Fix UI scaling
git add Assets/UI/MainMenu.prefab
git commit -m "fix(PROJ-999): fix UI scaling issues for release"
```

#### 4.3 Test Release
```bash
# Test all features in release branch
# - Build game
# - Test on target platform
# - Check performance
# - Verify bug fixes
```

#### 4.4 Update Jira Release
- Create Jira release version "v1.0.0"
- Move completed tickets to release
- Update release notes
- Set release date

---

### Step 5: 🏷️ Finish Release

#### 5.1 Finish Release
```bash
# Finish release
git flow release finish 1.0.0

# Git will open editor to write tag message
# You need to write message describing the release
```

#### 5.2 Write Tag Message
**In editor, write message as follows:**

```
Release v1.0.0 - Player Movement System

Features included:
- Implemented WASD player movement controls
- Added player walk animation system
- Integrated input management system
- Created responsive UI controls

Bug fixes:
- Adjusted player movement speed for optimal gameplay
- Fixed UI scaling issues across different resolutions
- Resolved animation transition problems

Technical improvements:
- Optimized movement performance
- Enhanced collision detection
- Improved code structure

Build: Unity 2022.3 LTS
Target platforms: Windows, macOS
Release date: [Release date]
```

#### 5.3 After Saving Tag Message
```bash
# Git will automatically:
# 1. Create annotated tag: v1.0.0
# 2. Merge release into master
# 3. Merge release into develop
# 4. Delete release branch
# 5. Switch you back to develop branch

# Display summary:
# Summary of actions:
# - Release branch 'release/1.0.0' has been merged into 'master'
# - Tag 'v1.0.0' has been back-merged into 'develop'
# - Release branch 'release/1.0.0' has been locally deleted
# - You are now on branch 'develop'
```

#### 5.4 Push Everything to Remote
```bash
# Push develop
git push origin develop

# Push master
git push origin master

# Push tag
git push origin v1.0.0

# Or push all tags
git push origin --tags
```

#### 5.5 Final Jira Update
- Move ticket `PROJ-123` from "Done" → "Closed"
- Update release status in Jira
- Add git tag link to Jira release
- Update release notes with final information

---

### Step 6: 🔥 Emergency Bug Fix (If needed)

#### 6.1 Create Hotfix
```bash
# Create hotfix from master
git flow hotfix start PROJ-999-critical-bug-fix

# Result: Created branch hotfix/PROJ-999-critical-bug-fix from master
```

#### 6.2 Fix Critical Bug
```bash
# Fix the bug
git add Assets/Scripts/PlayerMovement.cs
git commit -m "fix(PROJ-999): fix critical game crash"

# Test hotfix
# Build and test on target platform
```

#### 6.3 Finish Hotfix
```bash
# Finish hotfix
git flow hotfix finish PROJ-999-critical-bug-fix

# Write tag message:
```

**Hotfix Tag Message:**
```
Hotfix v1.0.1 - Fix critical player crash issue

Bug fixes:
- Fixed player crash when moving at high speed
- Improved movement collision detection
- Enhanced error handling for edge cases

Critical fix for production stability.
Build: Unity 2022.3 LTS
Target: Windows, macOS
```

**Jira Hotfix Process:**
- Create urgent bug ticket in Jira (Priority: Critical/Blocker)
- Use special hotfix ticket prefix (e.g., PROJ-999)
- Update ticket status immediately when hotfix is deployed
- Link hotfix commit to the urgent ticket

---

## 🎯 Final Result

### 🌿 Branch Structure after Release:
```
master (stable) ← v1.0.0 tag
    ↑
develop (development)
    ↑
feature branches (new features)
```

### 🏷️ Tags Created:
- `v1.0.0` - Release with player movement system
- `v1.0.1` - Hotfix for critical bug (if any)

### 📊 Jira Status:
- `PROJ-123` - Closed (completed and released)
- Release version "v1.0.0" - Released

### 📁 Files Managed:
- `Assets/Scripts/PlayerMovement.cs` - Core movement logic
- `Assets/Scripts/InputManager.cs` - Input handling
- `Assets/Animations/PlayerWalk.anim` - Player animation
- `ProjectSettings/ProjectVersion.txt` - Version info

---

## 🎯 Unity-Specific Guidelines

### 📁 File Management
- **Scene files:** Only commit completed scenes, avoid committing scenes being edited
- **Meta files:** ALWAYS commit with corresponding assets
- **Build settings:** Commit `ProjectSettings.asset` when changing build config
- **Package manifest:** Commit `Packages/manifest.json` when adding/removing packages

### 🚫 Files to Ignore
```gitignore
# Unity generated files
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# MemoryCaptures can get excessive in size
[Mm]emoryCaptures/

# Asset meta data should only be ignored when the corresponding asset is also ignored
!/[Aa]ssets/**/*.meta

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.aab
*.unitypackage
*.app

# Crashlytics generated file
crashlytics-build.properties

# OS generated
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db
```

### 🔄 Scene Management
- **Main Scene:** Only edit when necessary
- **Test Scene:** Use for feature testing
- **Build Scene:** Final scene for build

---

## 🚨 Important Notes

### ✅ Should Do:
- Test thoroughly before finishing feature
- Commit frequently with clear messages
- Only fix minor bugs in release branch
- Write detailed and descriptive tag messages
- Update Jira after each step
- Sync with develop frequently
- Test features before merging
- Create Pull Request for every feature

### ❌ Should Not Do:
- Add new features in release branch
- Commit untested code
- Skip Jira updates
- Write overly brief tag messages
- Push incomplete code
- Merge code with compile errors
- Skip scene testing

### 🔧 Troubleshooting:
- **Merge conflicts:** Resolve conflicts in Unity Editor
- **Tag message editor:** Use vim/nano/VS Code to edit
- **Push failed:** Check network and permissions
- **Jira sync issues:** Manual update if needed
- **Scene conflicts:** Backup scene before merging
- **Meta file issues:** Regenerate meta files in Unity

---

## 📚 Commands Reference

### 🔍 Check Status:
```bash
git branch          # View current branch
git status          # View file status
git log --oneline   # View commit history
git tag -l          # View all tags
```

### 🔄 Branch Operations:
```bash
git flow feature start <name>     # Create feature
git flow feature finish <name>    # Finish feature
git flow release start <version>  # Create release
git flow release finish <version> # Finish release
git flow hotfix start <name>      # Create hotfix
git flow hotfix finish <name>     # Finish hotfix
```

### 📤 Push Operations:
```bash
git push origin <branch>          # Push branch
git push origin <tag>             # Push tag
git push origin --tags            # Push all tags
```

### 🏷️ Tag Operations:
```bash
git tag -a v1.0.0 -m "Release message"  # Create annotated tag
git show v1.0.0                        # View tag details
git tag -d v1.0.0                      # Delete tag local
git push origin --delete v1.0.0        # Delete tag remote
```

### 🧹 Cleanup:
```bash
git branch --merged | grep -v "\*" | xargs -n 1 git branch -d  # Clean merged branches
git remote prune origin                                           # Clean remote tracking
```

---

## 📞 Support

If you encounter issues with Git Flow:
1. Check this documentation first
2. Ask team lead or senior developer
3. Create an issue in the project repository
4. Refer to troubleshooting section

---

## 📅 Version History

- **v1.0.0** - Initial Git Flow setup
- **v1.1.0** - Added Unity-specific guidelines
- **v1.2.0** - Enhanced best practices and troubleshooting
- **v1.3.0** - Added Jira workflow integration
- **v1.4.0** - Added comprehensive Git Tag guidelines
- **v2.0.0** - Complete guide with step-by-step instructions

---

*This documentation combines Git Flow theory with detailed practical guidance, based on real-world experience and best practices for Unity projects.*
