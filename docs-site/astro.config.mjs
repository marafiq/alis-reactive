// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	site: 'https://marafiq.github.io',
	base: '/alis-reactive',
	integrations: [
		starlight({
			title: 'Alis.Reactive',
			customCss: ['./src/styles/custom.css'],
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
					label: 'Start',
					items: [
						{ label: 'Your First Plan', slug: 'getting-started/your-first-plan' },
					],
				},
				{
					label: 'Mental Model',
					items: [
						{ label: 'Reactive Plan', slug: 'csharp-modules/mental-model' },
						{ label: 'Events, Payloads, Members', slug: 'csharp-modules/reactivity/events-members-values' },
						{ label: 'HTTP and Validation', slug: 'csharp-modules/reactivity/http-and-validation' },
						{ label: 'Components', slug: 'components/overview' },
					],
				},
				{
					label: 'Reference',
					items: [
						{ label: 'Build and Verify', slug: 'reference/build-commands' },
					],
				},
			],
			expressiveCode: {
				themes: ['github-dark', 'github-light'],
			},
		}),
	],
});
