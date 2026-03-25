// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import astroD2 from 'astro-d2';

// https://astro.build/config
export default defineConfig({
	site: 'https://marafiq.github.io',
	base: '/alis-reactive',
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
					href: 'https://github.com/marafiq/alis-reactive',
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
					label: 'Examples',
					items: [
						{ label: 'Resident Intake Form', slug: 'examples/resident-intake' },
						{ label: 'Patterns of Reactivity', slug: 'examples/patterns' },
					],
				},
				{
					label: 'Features',
					items: [
						{ label: 'Reactive Mental Model', slug: 'csharp-modules/mental-model' },
						{ label: 'Plans and Rendering', slug: 'csharp-modules/plan-and-entries' },
						{
							label: 'Reactivity',
							items: [
								{ label: 'Triggers', slug: 'csharp-modules/reactivity/triggers-and-reactions' },
								{ label: 'Server-Sent Events', slug: 'csharp-modules/reactivity/server-push' },
								{ label: 'SignalR', slug: 'csharp-modules/reactivity/signalr' },
								{ label: 'Element Mutations', slug: 'csharp-modules/reactivity/element-mutations' },
								{ label: 'Component API', slug: 'csharp-modules/reactivity/component-api' },
								{ label: 'Conditions', slug: 'csharp-modules/reactivity/conditions' },
								{ label: 'HTTP Pipeline', slug: 'csharp-modules/reactivity/http-pipeline' },
								{ label: 'Validation', slug: 'csharp-modules/reactivity/validation' },
							],
						},
						{
							label: 'Components',
							items: [
								{ label: 'Overview', slug: 'components/overview' },
								{ label: 'Native', slug: 'components/native-components' },
								{ label: 'Syncfusion', slug: 'components/fusion-components' },
								{ label: 'Design System', slug: 'components/design-system' },
							],
						},
					],
				},
				{
					label: 'Architecture',
					items: [
						{ label: 'Overview', slug: 'architecture/three-layers' },
						{
							label: 'Fluent Builders',
							items: [
								{ label: 'Overview', slug: 'architecture/fluent-builders' },
								{ label: 'The Builders', slug: 'architecture/the-builders' },
								{ label: 'Descriptors & Plan', slug: 'architecture/descriptors-and-plan' },
								{ label: 'The JSON Plan Contract', slug: 'architecture/the-contract' },
								{ label: 'The Vertical Slice', slug: 'architecture/vertical-slice' },
							],
						},
						{ label: 'JSON Plan Schema', slug: 'architecture/json-plan-schema' },
						{
							label: 'Runtime',
							items: [
								{ label: 'Overview', slug: 'architecture/runtime' },
								{ label: 'Component Model', slug: 'architecture/component-model' },
								{ label: 'Error Behavior', slug: 'architecture/error-behavior' },
							],
						},
						{
							label: 'Plan Lifecycle',
							items: [
								{ label: 'Plan Composition', slug: 'architecture/plan-composition' },
							],
						},
					],
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
