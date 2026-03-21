// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import astroD2 from 'astro-d2';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Alis.Reactive',
			customCss: ['./src/styles/custom.css'],
			head: [
				{
					tag: 'script',
					content: `
						document.addEventListener('click', (e) => {
							const svg = e.target.closest('svg[data-d2-version]');
							if (svg) {
								const modal = document.createElement('div');
								modal.className = 'd2-modal';
								modal.innerHTML = svg.outerHTML;
								modal.addEventListener('click', () => modal.remove());
								document.addEventListener('keydown', function esc(e) {
									if (e.key === 'Escape') { modal.remove(); document.removeEventListener('keydown', esc); }
								});
								document.body.appendChild(modal);
							}
						});
					`,
				},
			],
			social: [
				{
					icon: 'github',
					label: 'GitHub',
					href: 'https://github.com/AlisTechnologies/alis-reactive',
				},
			],
			sidebar: [
				{ label: 'Why Alis.Reactive?', slug: 'why' },
				{
					label: 'Getting Started',
					items: [
						{ label: 'Your First Plan', slug: 'getting-started/your-first-plan' },
					],
				},
				{
					label: 'Features',
					items: [
						{ label: 'Reactive Mental Model', slug: 'csharp-modules/mental-model' },
						{ label: 'Plans and Rendering', slug: 'csharp-modules/plan-and-entries' },
						{ label: 'Triggers', slug: 'csharp-modules/triggers-and-reactions' },
						{
							label: 'Reactivity',
							items: [
								{ label: 'Element Mutations', slug: 'csharp-modules/reactivity/element-mutations' },
								{ label: 'Component API', slug: 'csharp-modules/reactivity/component-api' },
								{ label: 'Conditions', slug: 'csharp-modules/reactivity/conditions' },
								{ label: 'HTTP Pipeline', slug: 'csharp-modules/reactivity/http-pipeline' },
							],
						},
						{
							label: 'Components',
							items: [
								{ label: 'Overview', slug: 'components/overview' },
								{ label: 'Native', slug: 'components/native-components' },
								{ label: 'Syncfusion', slug: 'components/fusion-components' },
							],
						},
						{
							label: 'Testing',
							items: [
								{ label: 'Strategy', slug: 'testing/strategy' },
								{ label: 'Writing Tests', slug: 'testing/writing-tests' },
							],
						},
					],
				},
				{
					label: 'Architecture',
					autogenerate: { directory: 'architecture' },
				},
				{
					label: 'Runtime',
					autogenerate: { directory: 'runtime' },
				},
				{
					label: 'Reference',
					autogenerate: { directory: 'reference' },
				},
			],
			expressiveCode: {
				themes: ['github-dark', 'github-light'],
			},
		}),
		astroD2({
			skipGeneration: false,
			inline: true,
			pad: 40,
			theme: {
				default: '1',
				dark: false,
			},
		}),
	],
});
