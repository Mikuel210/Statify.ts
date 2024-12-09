import statify from "statify";

statify.compile({
	template: `
	
	hi

	<statify>
	console.log("hiiii")
	</statify>

	bye
	
	`,
});
