# PNG Resources Required for VSIX Build

## CRITICAL: VSIX will not build without these images

### Required Files:

1. **Icon.png**
   - Location: `src/SSMSSQLComplete/Resources/Icon.png`
   - Size: **90x90 pixels**
   - Format: PNG
   - Purpose: Extension icon shown in Visual Studio Extension Manager
   - Currently: Only placeholder .txt file exists

2. **Preview.png**
   - Location: `src/SSMSSQLComplete/Resources/Preview.png`
   - Size: **200x200 pixels** (recommended)
   - Format: PNG
   - Purpose: Preview image shown in Extension Manager details
   - Currently: Only placeholder .txt file exists

### How to Create:

**Option 1: Quick Placeholder (5 minutes)**
```bash
# Create simple colored squares as placeholders
# Linux/Mac with ImageMagick:
convert -size 90x90 xc:#0078D4 src/SSMSSQLComplete/Resources/Icon.png
convert -size 200x200 xc:#0078D4 src/SSMSSQLComplete/Resources/Preview.png

# Windows with Paint.NET or Photoshop:
# 1. Create new image 90x90 or 200x200
# 2. Fill with blue (#0078D4 - SQL Server blue)
# 3. Add text "SQL" in white
# 4. Save as PNG
```

**Option 2: Professional Design (30-60 minutes)**
1. Design icon representing SQL/database + completion
2. Suggested elements:
   - Database cylinder icon
   - Lightning bolt (for speed)
   - Code brackets { }
   - IntelliSense light bulb
3. Use SQL Server blue (#0078D4) as primary color
4. Keep design simple and recognizable at small sizes

**Option 3: Use Online Tools (15 minutes)**
- Canva.com (free templates)
- Figma (free, professional)
- IconScout.com (find database icons)
- FlatIcon.com (free with attribution)

### Quick Fix to Build Without Images:

**Temporary workaround** (removes image references):

Edit `source.extension.vsixmanifest` and comment out:
```xml
<!-- <Icon>Resources\Icon.png</Icon> -->
<!-- <PreviewImage>Resources\Preview.png</PreviewImage> -->
```

**Note:** Extension will work but look unprofessional without icons.

### After Creating Images:

1. Delete the `.txt` placeholder files:
   ```bash
   rm src/SSMSSQLComplete/Resources/Icon.png.txt
   rm src/SSMSSQLComplete/Resources/Preview.png.txt
   ```

2. Place your PNG files in the Resources folder

3. Verify in project file that they're set to:
   - Build Action: Content
   - Copy to Output Directory: Always
   - Include in VSIX: true

4. Rebuild solution

### Status:

❌ **BLOCKED:** VSIX build will fail until PNG files are created

✅ **WORKAROUND:** Comment out icon references in manifest (looks unprofessional)

---

**Priority:** HIGH - Required for VSIX packaging
**Effort:** 5-60 minutes depending on approach chosen
**Blocking:** Yes - cannot create installable .vsix without this
