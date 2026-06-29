window.blazingTheme = (() => {
  const themes = new Set([
    'material-base',
    'material',
    'material-dark-base',
    'material-dark',
    'standard-base',
    'standard',
    'standard-dark-base',
    'standard-dark',
    'default-base',
    'default',
    'dark-base',
    'dark',
    'software-base',
    'software',
    'software-dark-base',
    'software-dark',
    'humanistic-base',
    'humanistic',
    'humanistic-dark-base',
    'humanistic-dark'
  ]);

  function apply(radzenTheme, variables) {
    const theme = themes.has(radzenTheme) ? radzenTheme : 'material-base';
    const themeLink = document.getElementById('blazing-radzen-theme');

    if (themeLink) {
      themeLink.href = `/_content/Radzen.Blazor/css/${theme}.css`;
    }

    const root = document.documentElement;
    for (const [name, value] of Object.entries(variables || {})) {
      root.style.setProperty(name, value);
    }
  }

  return { apply };
})();
