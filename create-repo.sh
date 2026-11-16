#!/bin/bash
# Script to create the GitHub repository and push code
# This requires GitHub CLI (gh) to be installed and authenticated

set -e

echo "Windows Toolbox - GitHub Repository Setup"
echo "=========================================="
echo ""

# Check if gh is installed
if ! command -v gh &> /dev/null; then
    echo "Error: GitHub CLI (gh) is not installed."
    echo "Please install it from: https://cli.github.com/"
    echo ""
    echo "Or create the repository manually at:"
    echo "https://github.com/new"
    exit 1
fi

# Check if gh is authenticated
if ! gh auth status &> /dev/null; then
    echo "GitHub CLI is not authenticated."
    echo "Running: gh auth login"
    echo ""
    gh auth login
fi

echo "Creating repository: rich-wisdom/windows-toolbox"
echo ""

# Create the repository
gh repo create rich-wisdom/windows-toolbox \
    --public \
    --description "Native Windows utility app with 102 professional tools. Built with WinUI 3 and .NET 8." \
    --source . \
    --remote origin \
    --push

echo ""
echo "✅ Repository created successfully!"
echo ""
echo "Repository URL: https://github.com/rich-wisdom/windows-toolbox"
echo "Branch: main"
echo "Commits: 3"
echo ""
echo "Next steps:"
echo "1. Visit https://github.com/rich-wisdom/windows-toolbox"
echo "2. Add topics/tags for discoverability"
echo "3. Configure branch protection rules"
echo "4. Start implementing more tools!"
