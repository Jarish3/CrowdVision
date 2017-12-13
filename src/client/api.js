function handleErrors(response) {
	if (!response.ok) {
		throw Error(response.statusText);
	}
	return response;
}

function getMense() {
	return fetch('/api/Mense')
		.then(handleErrors)
		.then(response => response.json())
		.catch(err => alert(err));
}

export default {
	getMense
};
