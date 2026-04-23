using Godot;

public partial class ItemTooltip : Node3D
{
    private const float PixelsPerUnit = 250f;
    private const int FixedWidth = 300;

    [Export] private SubViewport _viewport;
    [Export] private PanelContainer _panel;
    [Export] private Label _nameLabel;
    [Export] private Label _rarityLabel;
    [Export] private Label _typeLabel;
    [Export] private HSeparator _separator;
    [Export] private Label _descLabel;
    [Export] private MeshInstance3D _quad;

    public void Init(ItemResource resource)
    {
        Color rarityColor = GetRarityColor(resource.Rarity);

        _nameLabel.Text = resource.Name;
        _nameLabel.AddThemeColorOverride("font_color", rarityColor);

        _rarityLabel.Text = GetRarityName(resource.Rarity);
        _rarityLabel.AddThemeColorOverride("font_color", rarityColor);

        _typeLabel.Text = resource.Behavior.ToString();
        _typeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));

        bool hasDesc = !string.IsNullOrEmpty(resource.Description);
        _descLabel.Text = resource.Description ?? "";
        _descLabel.Visible = hasDesc;
        _separator.Visible = hasDesc;
        _descLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.75f));

        _panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(rarityColor.R * 0.12f, rarityColor.G * 0.12f, rarityColor.B * 0.12f, 0.88f),
            BorderColor = new Color(rarityColor.R * 0.6f, rarityColor.G * 0.6f, rarityColor.B * 0.6f, 0.95f),
            BorderWidthLeft = 3,
            BorderWidthRight = 3,
            BorderWidthTop = 3,
            BorderWidthBottom = 3,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
        });

        _quad.MaterialOverride = new StandardMaterial3D
        {
            AlbedoTexture = _viewport.GetTexture(),
            BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            RenderPriority = 1,
        };

        // Set width immediately so the very first layout pass wraps text at the right width.
        _viewport.Size = new Vector2I(FixedWidth, 2000);
        _panel.Resized += OnPanelResized;
        Visible = false;
    }

    private void OnPanelResized()
    {
        int height = (int)_panel.Size.Y;
        if (height <= 0 || height >= 2000) return;

        _panel.Resized -= OnPanelResized;

        _viewport.Size = new Vector2I(FixedWidth, height);

        var mesh = (QuadMesh)_quad.Mesh;
        mesh.Size = new Vector2(FixedWidth, height) / PixelsPerUnit;
        _quad.Position = new Vector3(mesh.Size.X + 1f, 0, 0);
    }

    private static Color GetRarityColor(int rarity) => rarity switch
    {
        1 => Colors.White,
        2 => new Color(0.1f, 0.85f, 0.1f), // Green
        3 => new Color(0.2f, 0.5f, 1.0f), // Blue
        4 => new Color(0.7f, 0.2f, 0.95f), // Purple
        5 => new Color(1.0f, 0.65f, 0.0f), // Gold
        _ => Colors.White
    };

    private static string GetRarityName(int rarity) => rarity switch
    {
        1 => "Common",
        2 => "Uncommon",
        3 => "Rare",
        4 => "Epic",
        5 => "Legendary",
        _ => "Unknown"
    };
}
