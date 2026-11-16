#!/bin/bash
# Alternative script to create GitHub repository using REST API
# Requires a GitHub Personal Access Token with 'repo' scope

set -e

echo "Windows Toolbox - GitHub Repository Creation via API"
echo "====================================================="
echo ""

# Check if GITHUB_TOKEN is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "Error: GITHUB_TOKEN environment variable is not set."
    echo ""
    echo "To create a token:"
    echo "1. Go to https://github.com/settings/tokens/new"
    echo "2. Give it a name: 'windows-toolbox-creation'"
    echo "3. Select scope: 'repo' (Full control of private repositories)"
    echo "4. Click 'Generate token'"
    echo "5. Copy the token and run:"
    echo ""
    echo "   export GITHUB_TOKEN='your_token_here'"
    echo "   ./create-repo-api.sh"
    echo ""
    exit 1
fi

echo "Creating repository via GitHub API..."
echo ""

# Create the repository
response=$(curl -s -w "\n%{http_code}" \
    -X POST \
    -H "Authorization: token $GITHUB_TOKEN" \
    -H "Accept: application/vnd.github.v3+json" \
    https://api.github.com/user/repos \
    -d '{
        "name": "windows-toolbox",
        "description": "Native Windows utility app with 102 professional tools. Built with WinUI 3 and .NET 8.",
        "homepage": "https://github.com/rich-wisdom/windows-toolbox",
        "private": false,
        "has_issues": true,
        "has_projects": true,
        "has_wiki": true,
        "auto_init": false
    }')

http_code=$(echo "$response" | tail -n1)
response_body=$(echo "$response" | sed '$d')

if [ "$http_code" = "201" ]; then
    echo "✅ Repository created successfully!"
    echo ""
    echo "Pushing code to GitHub..."
    git push -u origin main
    echo ""
    echo "✅ All done!"
    echo ""
    echo "Repository URL: https://github.com/rich-wisdom/windows-toolbox"
    echo "View your repository at the URL above."
elif [ "$http_code" = "422" ]; then
    echo "⚠️  Repository may already exist or name is invalid."
    echo "Response: $response_body"
    echo ""
    echo "Try pushing anyway:"
    echo "git push -u origin main"
else
    echo "❌ Failed to create repository (HTTP $http_code)"
    echo "Response: $response_body"
    echo ""
    echo "Try using GitHub CLI instead:"
    echo "./create-repo.sh"
fi
